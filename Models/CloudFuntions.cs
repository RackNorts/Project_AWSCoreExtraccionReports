 using Amazon.IdentityManagement.Model;
using Amazon.IdentityManagement;
using Amazon.Organizations.Model;
using Amazon.Organizations;
using Amazon.ResourceGroupsTaggingAPI.Model;
using Amazon.ResourceGroupsTaggingAPI;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Amazon.S3;

namespace AWSCoreExtraccionReports.Models
{
    public class CloudFuntions : IDisposable
    {
        //Función para listar todas las cuentas de la organización
        public async Task<List<Account>> ListAllAccounts(AmazonOrganizationsClient orgClient)
        {
            List<Account> accountsList = new();
            string? nextToken = null;
            do
            {
                ListAccountsRequest request = new ListAccountsRequest { NextToken = nextToken };
                ListAccountsResponse response = await orgClient.ListAccountsAsync(request);
                accountsList.AddRange(response.Accounts);
                nextToken = response.NextToken;
            } while (nextToken != null);
            return accountsList;
        }

        //Funcion para listar los usuarios de la consola
        public async Task<List<User>> ListAllUsers(AmazonIdentityManagementServiceClient iamClient)
        {
            List<User> user = new();
            string? marker = null;
            do
            {
                ListUsersResponse response = await iamClient.ListUsersAsync(new ListUsersRequest { Marker = marker });
                user.AddRange(response.Users);
                marker = response.Marker;
            } while (marker != null);
            return user;
        }

        //Funcion para listar las politicas por Usuario
        public async Task<List<AttachedPolicyType>> ListPolicyUser(AmazonIdentityManagementServiceClient iamClient,string userName)
        {
            List<AttachedPolicyType> policyTypes = new();
            string? maker = null;

            do
            {
                var response = await iamClient.ListAttachedUserPoliciesAsync(new ListAttachedUserPoliciesRequest { UserName = userName, Marker = maker });
                policyTypes.AddRange(response.AttachedPolicies);
                maker = response.Marker;
            } while (maker != null);

            return policyTypes;
        }
        
        //Funcion para obtener la lista de políticas administradas en AWS
        public async Task<List<ManagedPolicy>> ListPolicy(AmazonIdentityManagementServiceClient iamClient)
        {
            List<ManagedPolicy> policies = new();
            string? marker = null;
            do
            {
                Amazon.IdentityManagement.Model.ListPoliciesResponse response = await iamClient.ListPoliciesAsync(new Amazon.IdentityManagement.Model.ListPoliciesRequest { Scope = PolicyScopeType.All, Marker = marker });
                policies.AddRange(response.Policies);
                marker = response.Marker;
            }
            while (!string.IsNullOrEmpty(marker));
            return policies;
        }

        //Funcion de Retorno para la configuracion del JSON de los Policy
        public async Task<string> getPolicyJSon(AmazonIdentityManagementServiceClient iamClient, ManagedPolicy policy)
        {
            GetPolicyVersionResponse response = await iamClient.GetPolicyVersionAsync(new GetPolicyVersionRequest { PolicyArn = policy.Arn, VersionId = policy.DefaultVersionId });
            return JObject.Parse(HttpUtility.UrlDecode(response.PolicyVersion.Document)).ToString(Formatting.None);
        }

        //Metodo para extraer el tipo de recurso desde el ARN
        public string GetResourceTypeFromARN(string arn)
        {
            if (string.IsNullOrEmpty(arn)) return "ARN No encontrado";
            var parts = arn.Split(':');
            return parts.Length  <= 6 ? "Desconocido" : parts[2];
        }

        // Función para obtener todos los recursos de la cuenta
        public async Task<List<ResourceTagMapping>> GetTaggedResources(AmazonResourceGroupsTaggingAPIClient taggingClient)
        {
            List<ResourceTagMapping> resourceList = new();
            string? paginationToken = null;

            do
            {
                GetResourcesRequest request = new GetResourcesRequest { PaginationToken = paginationToken };
                GetResourcesResponse response = await taggingClient.GetResourcesAsync(request);
                resourceList.AddRange(response.ResourceTagMappingList);
                paginationToken = response.PaginationToken;
            } while (!string.IsNullOrEmpty(paginationToken));

            return resourceList;
        }

        //Funcion para obtener los servicios habilitado por Cuentea
        public async Task<List<EnabledServicePrincipal>> ListEnabledServicesForAccount(AmazonOrganizationsClient orgClient, string accountId)
        {
            List<EnabledServicePrincipal> enabledServices = new();
            string? nextToken = null;

            do
            {
                var request = new ListAWSServiceAccessForOrganizationRequest
                {
                    NextToken = nextToken
                };

                var response = await orgClient.ListAWSServiceAccessForOrganizationAsync(request);

                foreach (var service in response.EnabledServicePrincipals)
                {
                    enabledServices.Add(service);
                }

                nextToken = response.NextToken;

            } while (!string.IsNullOrEmpty(nextToken));

            return enabledServices;
        }

        public void Dispose()
        {

        }
    }
}