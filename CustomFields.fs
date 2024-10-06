namespace JCC

open APICalls

module CustomFields =
    let GetAllCustomFields () = client.Fields.GetCustomFieldsAsync() |> call