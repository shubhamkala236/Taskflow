export const environment = {
  production: true,
  apiUrl: 'https://taskflow-api-v-auadaeguh6b0fhcw.centralindia-01.azurewebsites.net',
  msal: {
    clientId: 'a3691422-691a-4982-8249-98663a65490d',
    authority: 'https://login.microsoftonline.com/dd79b9fb-5f86-4616-9ff9-ac4cde607635',
    redirectUri: 'https://mango-forest-03b8a7f00.7.azurestaticapps.net',
    apiScope: 'api://taskflow-api-entra/Tasks.ReadWrite'
  }
};