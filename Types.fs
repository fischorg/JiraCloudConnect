namespace JiraCloudConnect

open FSharp
open FSharp.Data
open System

[<AutoOpen>]
module Types =     
    type ProjectResponse = JsonProvider<"c:\dev\JiraCloudConnect\samples\project.sample.json">


    // Helper types
    type AvatarUrls = 
        { 
            ``16x16`` : string
            ``24x24`` : string
            ``32x32`` : string
            ``48x48`` : string
        }

    type User = 
        { 
            AccountId : string
            AccountType : string
            Active : bool
            AvatarUrls : AvatarUrls
            DisplayName : string
            Name : string
            Self : string
        }

    type IssueType = 
        { 
            AvatarId : int
            Description : string
            HierarchyLevel : int
            IconUrl : string
            Id : int
            Name : string
            Self : string
            Subtask : bool
        }

    // Main type
    type Project = 
        { 
            AssigneeType : string
            AvatarUrls : AvatarUrls
            Description : string
            Email : string
            Id : int
            IssueTypes : IssueType list
            Key : string
            Lead : User
            Name : string
            //ProjectCategory : ProjectCategory
            Self : string
            Simplified : bool
            Style : string
            Url : string
        }

    and Insight = 
        { 
            LastIssueUpdateTime : DateTime
            TotalIssueCount : int
        }

    and ProjectCategory = 
        { 
            Description : string
            Id : int
            Name : string
            Self : string
        }

    let BuildProject (project: ProjectResponse.Root) = 
        { 
            AssigneeType = project.AssigneeType
            AvatarUrls = 
                {
                    ``16x16`` = project.AvatarUrls.``16x16``
                    ``24x24`` = project.AvatarUrls.``24x24``
                    ``32x32`` = project.AvatarUrls.``32x32``
                    ``48x48`` = project.AvatarUrls.``48x48``
                }
            Description = project.Description
            Email = project.Email
            Id = project.Id
            IssueTypes = 
                project.IssueTypes
                |> Seq.map (fun it -> 
                    {
                        AvatarId = it.AvatarId
                        Description = it.Description
                        HierarchyLevel = it.HierarchyLevel
                        IconUrl = it.IconUrl
                        Id = it.Id
                        Name = it.Name
                        Self = it.Self
                        Subtask = it.Subtask
                    }
                )
                |> Seq.toList
            Key = project.Key
            Lead = 
                {
                    AccountId = project.Lead.AccountId
                    AccountType = project.Lead.AccountType
                    Active = project.Lead.Active
                    AvatarUrls = 
                        {
                            ``16x16`` = project.Lead.AvatarUrls.``16x16``
                            ``24x24`` = project.Lead.AvatarUrls.``24x24``
                            ``32x32`` = project.Lead.AvatarUrls.``32x32``
                            ``48x48`` = project.Lead.AvatarUrls.``48x48``
                        }
                    DisplayName = project.Lead.DisplayName
                    Name = project.Lead.Name.ToString()
                    Self = project.Lead.Self
                }
            Name = project.Name
            //ProjectCategory = 
            //    {
            //        Description = project.ProjectCategory.Description
            //        Id = project.ProjectCategory.Id
            //        Name = project.ProjectCategory.Name
            //        Self = project.ProjectCategory.Self
            //    }
            Self = project.Self
            Simplified = project.Simplified
            Style = project.Style
            Url = project.Url
        }
    let ParseProject (json: string) = ProjectResponse.Parse(json)