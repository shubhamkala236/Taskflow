export const environment = {
  production: false,
  apiUrl: 'https://localhost:7044',
  msal: {
    clientId: 'a3691422-691a-4982-8249-98663a65490d',
    authority: 'https://login.microsoftonline.com/dd79b9fb-5f86-4616-9ff9-ac4cde607635',
    redirectUri: 'http://localhost:4200',
    apiScope: 'api://taskflow-api-entra/Tasks.ReadWrite'
  }
};
