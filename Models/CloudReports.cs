using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Organizations;
using Amazon.Organizations.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;

namespace AWSCoreExtraccionReports.Models
{
    public class CloudReports : IDisposable
    {
        private string? ReportName = string.Empty;
        private List<string>? reportsLine = null;
        private CloudFuntions cloudFuntions = new();
        public CloudFuntions CloudFuntions { get => cloudFuntions; set => cloudFuntions = value; }


        //Funcino de ejecutar el reporte de usuarios y permisos asignados.
        public async Task<List<AccountUsers>> ReportUserConsole(string AWSConnection)
        {
            reportsLine = ["UserName|ARN|DateCreate|Permissions"];
            ReportName = $"Cofiguracion_Usuarios_{DateTime.Now:ddMMyyyy}.txt";
            
            List<AccountUsers> Report = [];

            SharedCredentialsFile sharedFile = new SharedCredentialsFile();
            if (!sharedFile.TryGetProfile(AWSConnection, out var profile))
            {
                throw new Exception($"Perfil ''{AWSConnection}'' no encontroado.....");
            }
            var  Credentials = AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);
            using (AmazonIdentityManagementServiceClient iamClient = new(Credentials, RegionEndpoint.USEast1))
            {
                List<User> user = await CloudFuntions.ListAllUsers(iamClient);
                AccountUsers? model = null;
                foreach (var item in user)
                {
                    List<AttachedPolicyType> policyTypes = await CloudFuntions.ListPolicyUser(iamClient, item.UserName);
                    if (policyTypes.Count == 0)
                    {
                        reportsLine.Add(string.Format("{0}|{1}|{2}|NO_POLICIES",item.UserName,item.Arn,item.CreateDate.Value.ToString("MM/dd/yyyy HH:mm")));
                    }

                    foreach(var policy in policyTypes)
                    {
                        model = new()
                        {
                            UserName = item.UserName.ToLower(),
                            ARN = item.Arn.ToLower(),
							UserCreate = item.CreateDate.Value.ToString("MM/dd/yyyy HH:mm"),
                            Permissions = policy.PolicyName.ToString()
                        };


                        reportsLine.Add(string.Format("{0}|{1}|{2}|{3}",model.UserName,model.ARN,model.UserCreate,model.Permissions));
                        Report.AddRange(model);
                    }
                }
            }
            
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, ReportName), reportsLine);
            //await LoadS3Reports(AWSConnection, "reportsecurity", Path.Combine(Environment.CurrentDirectory, ReportName));
            return Report;
        }

        //Funcion de ejecutar el reporte de cuentas vs PermissionSet
        public async Task<List<AccountPermissionSetAndPolicy>> ReportAccountVSPermissionSetPolicy(string AWSConnection)
        {
            reportsLine = ["Account|NamePolicy|JSONConfigurationPolicy"];
            ReportName = $"Cofiguracion_Cuenta_PermissionSet_{DateTime.Now:ddMMyyyy}.txt";

            List<AccountPermissionSetAndPolicy> Report = [];
			SharedCredentialsFile sharedFile = new SharedCredentialsFile();
			if (!sharedFile.TryGetProfile(AWSConnection, out var profile))
			{
				throw new Exception($"Perfil ''{AWSConnection}'' no encontroado.....");
			}
			var Credentials = AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);
			//StoredProfileAWSCredentials Credentials = new(AWSConnection);
            AmazonIdentityManagementServiceClient iamClient = new(Credentials, RegionEndpoint.USEast1);
            using (AmazonOrganizationsClient OrgClient = new(Credentials, RegionEndpoint.USEast1))
            {
                List<Account> Accounts = await CloudFuntions.ListAllAccounts(OrgClient);
                AccountPermissionSetAndPolicy? model = null;

                foreach (var account in Accounts)
                {
                    model = new();
                    var policies = await CloudFuntions.ListPolicy(iamClient);
                    foreach (var policy in policies)
                    {
                        model.Account = account.Id;
                        model.NamePolicy = policy.PolicyName.ToString();
                        model.JSonConfigurationPolicy = await CloudFuntions.getPolicyJSon(iamClient, policy);
                        reportsLine.Add(string.Format("{0}|{1}|{2}", model.Account, model.NamePolicy, model.JSonConfigurationPolicy));
                        Report.AddRange(model);
                    }

                }
            }

            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, ReportName), reportsLine);
            //await LoadS3Reports(AWSConnection, "reportsecurity", Path.Combine(Environment.CurrentDirectory, ReportName));
            return Report;
        }

        //Funcion de ejecutar el reporte de cuentas vs Servicios Activos
        public async Task<List<AccountService>> ReportAccountVSAvailableService(string AWSConnection)
        {
            reportsLine = ["Account|NameAccount|Service|State"];
            ReportName = $"Cofiguracion_Cuenta_Servicios_Activos_{DateTime.Now:ddMMyyyy}.txt";

            List<AccountService> report = [];
			SharedCredentialsFile sharedFile = new SharedCredentialsFile();
			if (!sharedFile.TryGetProfile(AWSConnection, out var profile))
			{
				throw new Exception($"Perfil ''{AWSConnection}'' no encontroado.....");
			}
			var Credentials = AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);
			using (AmazonOrganizationsClient orgClient = new(Credentials, RegionEndpoint.USEast1))
            {
                List<Account> accounts = await CloudFuntions.ListAllAccounts(orgClient);
                AccountService? model = null;
                int delayBetweenCallsMs = 500;

                foreach (Account account in accounts)
                {
                    try
                    {
                        List<EnabledServicePrincipal> services = await CloudFuntions.ListEnabledServicesForAccount(orgClient, account.Id);
                        foreach (EnabledServicePrincipal service in services)
                        {
                            model = new()
                            {
							    Account = account.Id,
                                NameAccount = account.Name.ToLower(),
                                Service = service.ServicePrincipal,
                                State = account.Status
                            };

						    reportsLine.Add(string.Format("{0}|{1}|{2}|{3}",model.Account,model.NameAccount,model.Service,model.State));
                            report.AddRange(model);
                        }
                    } catch (AmazonOrganizationsException ex) when (ex.ErrorCode == "TooManyRequestsException")
                    {
                        Console.WriteLine($"throttling para cuenta {account.Id} Aumentando delay....");
                        delayBetweenCallsMs += 100;
                        await Task.Delay(1000);
                        continue;
                    }

                    await Task.Delay(delayBetweenCallsMs);
                }
            }

            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, ReportName), reportsLine);
            return report;
        }

        //Funcion de ejecutar la carga de archivo a un bucket de S3
        public static async Task LoadS3Reports(string AWSConnection, string bucketName, string filereport)
        {
			SharedCredentialsFile sharedFile = new SharedCredentialsFile();
			if (!sharedFile.TryGetProfile(AWSConnection, out var profile))
			{
				throw new Exception($"Perfil ''{AWSConnection}'' no encontroado.....");
			}
			var Credentials = AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);

			//StoredProfileAWSCredentials Credentials = new (AWSConnection);
			AmazonS3Client s3Client = new(Credentials, RegionEndpoint.USEast1);
            try
            {

                if(!File.Exists(filereport))
                {
                    Console.Write($"Error: El Archivo {filereport} no existe");
                    return;
                }
                var putRequest = new PutObjectRequest {
                    BucketName = bucketName,
                    Key = Path.GetFileName(filereport),
                    FilePath = filereport,
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);
                Console.WriteLine($"HttpCodeResponse: {response.HttpStatusCode} \ncargado correctamente al bucket: {bucketName}");
            }
            catch (AmazonS3Exception s3Ex)
            {
                Console.WriteLine($"Error S3: {s3Ex.Message}");
            }
            catch (AmazonServiceException serviceEx)
            {
                Console.WriteLine($"Error de AWS Service: {serviceEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir archivo: {ex.Message}");
            }
        }


        //Listado  Recursos EC2
        public static async Task<List<string>> ResoruceToEC2(string AWSConnection)
        {
			SharedCredentialsFile sharedFile = new SharedCredentialsFile();
			if (!sharedFile.TryGetProfile(AWSConnection, out var profile))
			{
				throw new Exception($"Perfil ''{AWSConnection}'' no encontroado.....");
			}
			var Credentials = AWSCredentialsFactory.GetAWSCredentials(profile, sharedFile);
			//StoredProfileAWSCredentials Credentials = new(AWSConnection);
			using AmazonEC2Client EC2Client = new(Credentials, RegionEndpoint.USEast1);

            List<string> Report = [];
            AccountResources model = new();

            DescribeInstancesResponse instances = await EC2Client.DescribeInstancesAsync();
            foreach (var reservation in instances.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {

                }
            }

            return Report;
        }

        //Listado Recursos S3
        public static void ResourceS3(string AWSConnection) { }

        //Listado Recursos DynamoDb & Aurora
        public static void ResourceDynamoAurora(string AWSConnection) { }

        //Listado Recursos EKS
        public static void ResourceEKS(string AWSConnection) { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            CloudFuntions.Dispose();  
        }
    }
}