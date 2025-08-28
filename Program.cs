using AWSCoreExtraccionReports.Models;

// Cargar credenciales desde perfil AWS
using(CloudReports cloud = new())
{

    Console.WriteLine("Ejecucion de Cofiguracion_Cuenta_PermissionSet...");
    List<AccountPermissionSetAndPolicy> accounts = await cloud.ReportAccountVSPermissionSetPolicy("AWSLocalUser");
    Console.WriteLine($"Total de Registros: {accounts.Count}\nEjecucion de Cofiguracion_Cuenta_PermissionSet Finalizada...");

    Console.WriteLine("\nEjecucion de Cofiguracion_Cuenta_Servicios_Activos...");
    List<AccountService> service = await cloud.ReportAccountVSAvailableService("AWSLocalUser");
    Console.WriteLine($"Total de Registros: {service.Count}\nEjecucion de Cofiguracion_Cuenta_Servicios Finalizada...");

    Console.WriteLine("Ejecucion de Configuracion_Usuarios...");
    List<AccountUsers> users = await cloud.ReportUserConsole("AWSLocalUser");
    Console.WriteLine($"Total de Registros: {users.Count}\nEjecucion de Configuracion_Usuarios Finalizada...");
}