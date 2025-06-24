﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor.Entidades
{
    public class Transferencia
    {
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
        public string CuentaOrigen { get; set; }
        public string CuentaDestino { get; set; }
    }
}
