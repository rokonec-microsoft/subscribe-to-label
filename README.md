# Subscribe To Label service

This is asp.net core web application with one running Hosted Service, which is to be integrated with GitHub App and uses GitHub API identity to call GitHub APIs (v3 - REST).

Configuration of label subscriptions is stored in given repositories. Example: https://github.com/rokonec-int-tests/issue-notify-tests/blob/master/.github/AREAOWNERS.json. When it receives WebHook about isseu labeling, it will upsert comment mentioning given people.

## Test localhost github app integration by webhooks

For this to work given GitHub app has to set its webhook url to https://smee.io/zlcz3wSWRkhOywV (or new smee channel).
This will allow to recevie and debug GitHub app webhooks - in our particular case webhook about issues labeling

```
npm config set strict-ssl false
smee --url https://smee.io/zlcz3wSWRkhOywV --target http://localhost:3000/api/webhooks/incoming/github
```
## Run locally

### Set secrets
Right clict WEB project -> Manage User Secrets

Edit its content with following template
```
{
  "WebHooks:GitHub:SecretKey:default": "16charssecrets",
  "GitHubApp": {
    "ApplicationId": 123456,
    "InstallationId": 12345,
    "PrivateKey": {
      "KeyString": "MI...Tgw"
    }
  }
}
```

KeyString is .pem file (private key of GitHub app) without comment lines and \n

## Integration tests
Right clict test project -> Manage User Secrets

Edit its content with following template
```
{
  "WebHooks:GitHub:SecretKey:default": "16charssecrets",
  "GitHubApp": {
    "ApplicationId": 123456,
    "InstallationId": 12345,
    "PrivateKey": {
      "KeyString": "MI...Tgw"
    }
  }
}
```

It is possible to use different GitHub app installtion for local debug and integration tests if needed.
