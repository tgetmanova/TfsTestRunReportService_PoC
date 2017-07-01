# This is DRAFT of two approaches for customizing test run results derived from the TFS builds (XAML, vNext) step.
# !! PoC - just to prove it is possible.

- The general project uses libraries from standart object model
references:  
https://blogs.msdn.microsoft.com/buckh/2015/08/10/nuget-packages-for-tfs-and-visual-studio-online-net-client-object-model/  

 -`Microsoft.TeamFoundation.Client` - general logic of Tfs connection context (https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.client(v=vs.120).aspx ),  
 -`Microsoft.TeamFoundation.Build.Client` - for Build and Build Definition (https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.build.client(v=vs.120).aspx),  
 -`Microsoft.TeamFoundation.TestManagement.Client` - test run and test statuses management (https://msdn.microsoft.com/en-us/library/microsoft.teamfoundation.testmanagement.client.aspx),  
 
  
- The REST project combines new TFS REST API opportunities (https://www.visualstudio.com/en-us/docs/integrate/api/tfs/processes) wih the legacy object model client.  
References:  
https://stackoverflow.com/questions/38378106/what-is-the-difference-between-team-foundation-servers-client-and-webapi-li  
