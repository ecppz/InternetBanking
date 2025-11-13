using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Enums.Extensions
{
        public static class SavingsAccountStatusExtensions
        {
            public static bool IsOperable(this SavingsAccountStatus status)
            {
                return status == SavingsAccountStatus.Activa;
            }

            public static string ToVisualLabel(this SavingsAccountStatus status)
            {
                return status switch
                {
                    SavingsAccountStatus.Activa => "Activa",
                    SavingsAccountStatus.Cancelada => "Cancelada",
                    SavingsAccountStatus.Bloqueada => "Bloqueada",
                    SavingsAccountStatus.Suspendida => "Suspendida",
                    SavingsAccountStatus.Pendiente => "Pendiente",
                    _ => "Desconocido"
                };
            }
        }
}
