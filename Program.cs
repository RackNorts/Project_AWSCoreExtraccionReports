using AWSCoreExtraccionReports.Models;

// Cargar credenciales desde perfil AWS
using(CloudReports cloud = new())
{
    string ACCT = "";
    
    //Console.WriteLine("Ejecucion de Cofiguracion_Cuenta_PermissionSet...");
    //List<AccountPermissionSetAndPolicy> accounts = await cloud.ReportAccountVSPermissionSetPolicy(ACCT);
    //Console.WriteLine($"Total de Registros: {accounts.Count}\nEjecucion de Cofiguracion_Cuenta_PermissionSet Finalizada...");

    Console.WriteLine("\nEjecucion de Cofiguracion_Cuenta_Servicios_Activos...");
    List<AccountService> service = await cloud.ReportAccountVSAvailableService(ACCT);
    Console.WriteLine($"Total de Registros: {service.Count}\nEjecucion de Cofiguracion_Cuenta_Servicios Finalizada...");

    //Console.WriteLine("Ejecucion de Configuracion_Usuarios...");
    //List<AccountUsers> users = await cloud.ReportUserConsole(ACCT);
    //Console.WriteLine($"Total de Registros: {users.Count}\nEjecucion de Configuracion_Usuarios Finalizada...");

}
