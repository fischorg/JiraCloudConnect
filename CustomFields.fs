namespace JiraCloudConnect

open APICalls

module CustomFields =
    let GetAllCustomFields () = client.Fields.GetCustomFieldsAsync() |> call