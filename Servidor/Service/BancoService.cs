using Servidor.Entidades;
using Servidor.Logica;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor.Service
{
    public class BancoService
    {
        public bool CuentaExiste(string numeroCuenta)
        {
            using (var con = new SqlConnection(ConexionBD.Cadena))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM CUENTAS WHERE NUM_CUE = @num", con))
                {
                    cmd.Parameters.AddWithValue("@num", numeroCuenta);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public decimal ObtenerSaldo(string numeroCuenta)
        {
            using (var con = new SqlConnection(ConexionBD.Cadena))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT SAL_CUE FROM CUENTAS WHERE NUM_CUE = @num", con))
                {
                    cmd.Parameters.AddWithValue("@num", numeroCuenta);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }
    

        public bool Transferir(Transferencia t)
        {
            using (var con = new SqlConnection(ConexionBD.Cadena))
            {
                con.Open();
                using (var tran = con.BeginTransaction(System.Data.IsolationLevel.Serializable)) // I -> AISLAMIENTO
                {
                    try
                    {
                        // A y C -> Verificamos que exista la cuenta origen y tenga saldo suficiente (CONSISTENCIA)
                        var saldoCmd = new SqlCommand("SELECT SAL_CUE FROM CUENTAS WHERE NUM_CUE = @ori", con, tran);
                        saldoCmd.Parameters.AddWithValue("@ori", t.CuentaOrigen);
                        object result = saldoCmd.ExecuteScalar();

                        if (result == null)
                            throw new Exception("La cuenta origen no existe.");

                        decimal saldo = Convert.ToDecimal(result);
                        if (saldo == 0)
                            throw new Exception("Cuenta origen no tiene saldo disponible.");
                        if (saldo < t.Valor)
                            throw new Exception($"Saldo insuficiente. Disponible: {saldo}");

                        // A -> ATOMICIDAD: Todo se realiza como un bloque. Si algo falla, se hace rollback.
                        // Debitar a cuenta origen
                        var debito = new SqlCommand("UPDATE CUENTAS SET SAL_CUE = SAL_CUE - @val WHERE NUM_CUE = @ori", con, tran);
                        debito.Parameters.AddWithValue("@val", t.Valor);
                        debito.Parameters.AddWithValue("@ori", t.CuentaOrigen);
                        debito.ExecuteNonQuery();

                        // Verificar que exista la cuenta destino
                        var cuentaDestinoCmd = new SqlCommand("SELECT COUNT(*) FROM CUENTAS WHERE NUM_CUE = @des", con, tran);
                        cuentaDestinoCmd.Parameters.AddWithValue("@des", t.CuentaDestino);
                        int destinoExiste = (int)cuentaDestinoCmd.ExecuteScalar();
                        if (destinoExiste == 0)
                            throw new Exception("La cuenta destino no existe.");

                        // Acreditar a cuenta destino
                        var credito = new SqlCommand("UPDATE CUENTAS SET SAL_CUE = SAL_CUE + @val WHERE NUM_CUE = @des", con, tran);
                        credito.Parameters.AddWithValue("@val", t.Valor);
                        credito.Parameters.AddWithValue("@des", t.CuentaDestino);
                        credito.ExecuteNonQuery();

                        // Registrar transferencia
                        var insert = new SqlCommand(@"
                    INSERT INTO TRANSFERENCIAS (FEC_TRA, VALOR_TRA, NUM_CUE_ORI, NUM_CUE_DES)
                    VALUES (@fec, @val, @ori, @des)", con, tran);
                        insert.Parameters.AddWithValue("@fec", t.Fecha);
                        insert.Parameters.AddWithValue("@val", t.Valor);
                        insert.Parameters.AddWithValue("@ori", t.CuentaOrigen);
                        insert.Parameters.AddWithValue("@des", t.CuentaDestino);
                        insert.ExecuteNonQuery();

                        tran.Commit(); // D -> DURABILIDAD: Los cambios son permanentes
                        return true;
                    }
                    catch
                    {
                        tran.Rollback(); // A -> ATOMICIDAD: Si hay error, deshace todo
                        throw;
                    }
                }
            }
        }

    }

}
