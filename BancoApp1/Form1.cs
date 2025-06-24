using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Servidor.Entidades;
using Servidor.Service;


namespace BancoApp1
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private BancoService banco = new BancoService();

        private void btnVerificar_Click(object sender, EventArgs e)
        {
            var cuenta = txtCuenta.Text.Trim();
            var existe = banco.CuentaExiste(cuenta);

            if (existe)
            {
                lblEstado.Text = "✅ Cuenta válida";
                lblEstado.ForeColor = Color.CadetBlue;
                btnTransferir.Enabled = true;
                txtDestino.Enabled = true;
                txtValor.Enabled = true;
            }
            else
            {
                lblEstado.Text = "❌ La cuenta no existe";
                lblEstado.ForeColor = Color.Teal;
                btnTransferir.Enabled = false;
                txtDestino.Enabled = false;
                txtValor.Enabled = false;
            }
        }

        private void btnTransferir_Click(object sender, EventArgs e)
        {
            try
            {
                string cuentaOrigen = txtCuenta.Text.Trim();
                string cuentaDestino = txtDestino.Text.Trim();
                decimal valorTransferencia;

                // Verificar si el valor es numérico
                if (!decimal.TryParse(txtValor.Text, out valorTransferencia))
                {
                    lblResultado.Text = "⚠️ Ingrese un valor válido.";
                   lblResultado.ForeColor = Color.Tomato;
                    return;
                }

                // Verificar existencia de cuenta origen
                if (!banco.CuentaExiste(cuentaOrigen))
                {
                    lblResultado.Text = "❌ La cuenta de origen no existe.";
                    lblResultado.ForeColor = Color.Brown;
                    return;
                }

                // Verificar existencia de cuenta destino
                if (!banco.CuentaExiste(cuentaDestino))
                {
                    lblResultado.Text = "❌ La cuenta de destino no existe.";
                    lblResultado.ForeColor = Color.BlueViolet;
                    return;
                }

                // Crear objeto transferencia
                var transferencia = new Transferencia
                {
                    CuentaOrigen = cuentaOrigen,
                    CuentaDestino = cuentaDestino,
                    Valor = valorTransferencia,
                    Fecha = DateTime.Now
                };

                // Intentar transferir
                if (banco.Transferir(transferencia))
                {
                    lblResultado.Text = "✅ Transferencia realizada con éxito.";
                    lblResultado.ForeColor = Color.Bisque;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Saldo insuficiente"))
                {
                    // Obtener saldo actual
                    decimal saldo = banco.ObtenerSaldo(txtCuenta.Text.Trim());
                    lblResultado.Text = $"❌ Saldo insuficiente. Disponible: ${saldo:F2}";
                    lblResultado.ForeColor = Color.Azure;
                }
                else
                {
                    lblResultado.Text = "❌ Error: " + ex.Message;
                    lblResultado.ForeColor = Color.Aqua;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Load += Form1_Load;
            btnTransferir.Enabled = false;
            txtDestino.Enabled = false;
            txtValor.Enabled = false;
        }

        private void lblResultado_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblEstado_Click(object sender, EventArgs e)
        {

        }
    }
}
