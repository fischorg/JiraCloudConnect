namespace JiraCloudConnect

open FSharp.Core
open FSharp.Data

 module Webhooks =
    type WebhookIssueEvent = JsonProvider<"C:\dev\JiraCloudConnect\Samples\WebhookIssueEvent.json", RootName="WebhookIssueEvent">

    let ParseWebhook (data: string) = WebhookIssueEvent.Parse(data)