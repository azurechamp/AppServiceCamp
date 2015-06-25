# Web Demos

## Demo 1 - Web App Creation

This is a quick demo showing how quickly you can create a new Web App in the portal. Feel free to change alter this first demo.

1. Browse to [Preview Portal](https://portal.azure.com)
1. Click New / Web + Mobile / Web App.
1. Enter a unique name in the URL field and click the Create button.
1. While the site is being created, explain that Azure is provisioning a new Web App for you with supporting services, monitoring, support for continuous deployment, etc.
 > Note: This generally takes 30 - 60 seconds. During this time, you can ask them how long it would take their IT department or hosting provider to provision a new site for them. This us usually enough time for the new Web App to be created.
1. When the site comes up, scroll through the various services (Monitoring, Usage, Operations, Deployment, Networking) explaining that these are all live and have been provisioned with the Web App.
1. Click on the Browse button. When the default landing page loads, point out that the page illustrates the different options for publishing to the new site, including Git, FTP, Visual Studio, etc.

## Demo 2 - Language Creation (Hello All Worlds)

This demo shows all of the very many ways you can run code (OOTB) by default in Azure App Service Web App.

### Prerequisites

1. Go to http://portal.azure.com and provision a new free Web App.
 > Note: You can use the Web App you provisioned in the first demo here.
1. Open File Explorer and copy and paste the ftp host name into the address bar. You will be prompted to sign in. The ftp user name is also found in the portal ({websitename}\{username}). The password can be set by you in the portal.
1. Navigate to the folder site/wwwroot/. There should be one .html file in this folder corresponding to the default Website page for an empty slot.
1. Open another File Explorer and place them next to each other on screen. In this second File Explorer navigate to the Presentation\Web\Demo1 - Hello All Worlds\source\HelloAllWorlds folder in the DevCamp material.
1. Open the project in Visual Studio, enable restore packages from nuget and compile (to download all the packages required inside bin directory)
1. Below in Demo 1 you can now copy all of the files from File Explorer to File Explorer -> from your local disk to the Cloud Website simply by selecting all the files and dropping them on the website.

(An alternative here is to use the FTP client of your choosing but the effect is the same.)

### Demo

1. In the Azure Preview Portal, create a new Web App.
1. Expand the site, scroll to Configuration and click on Settings. Show that .NET, PHP, Python and Java are all shown.
1. "I've already created another site and connected using an FTP client." Show the Prerequisite web site. If you like you can even edit the .html page on the site to prove that it is indeed the live site!
1. Show the two File Explorer windows from the Prerequisites and explain that one is local disk while the other is the FTP Connection to the live Web App.
1. Copy the contents of the source directory into the /site/wwwroot.
1. Browse to the site and click through the index page to the samples running on the various platforms.

## Demo 3 - Visual Studio Support
1. File / New Web Application
1. Show Host In The Cloud dialog
1. Select Empty web site (for quick create)
1. Right-click project, select Publish
1. Show Web App creation
1. Cancel publish
1. Show Web App in Server Explorer
1. Right-click one Web App and show settings

## Demo 4 - Manual and Auto Scale

This is a simple portal demo.

1. Open an existing Web App (or provision a new one if necessary).
1. In the site settings, click the Scale tile (in the Usage section).
1. Demonstrate both manual and auto scale by toggling Autoscale mode between Off and Performance.
 > Note: Autoscale requires at least the Standard plan.

## Demo 5 - Redis Cache Demo

This demo shows using Redis Cache to make

> Setup for this demo takes up to 3 hours for the CacheFill project to run.

### Prerequisites

1. Create 2 Redis cache services on Azure
1. Ensure Redis cache services have enabled "NON-SSL PORT" - 6379 . This feature is disabled by default.
1. Add each Redis Cache Server connection details to:

	* CustomerQuery web.config
	* CacheFill app.config

1. Add Azure Storage connection string to:

	* CustomerQuery web.config
	* CacheFill app.config

1. Run CacheFill and follow the  instructions!

### Demo

1. Run the application.
1. Check the "Directly Search Table" checkbox.
1. Type a capital letter in the search field and click search. While the search is running (takes roughly 25 seconds) explain that the application is querying against 1 million un-indexed rows in table storage.
1. Uncheck the "Directly Search Table" checkbox and run the search again. Search completes in under 2 seconds because it's querying against an indexed Redis cache.

## Demo 6 - Redis Cache for ASP.NET Session State
This topic explains how to use the Azure Redis Cache Service for session state.

If your ASP.NET web app uses session state, you will need to configure an external session state provider (either the Redis Cache Service or a SQL Server session state provider). If you use session state, and don't use an external provider, you will be limited to one instance of your web app. The Redis Cache Service is the fastest and simplest to enable.

###Create the Cache
Follow [these directions](cache-dotnet-how-to-use-azure-redis-cache.md#create-cache) to create the cache.

###Add the RedisSessionStateProvider NuGet package to your web app
Install the NuGet `RedisSessionStateProvider` package.  Use the following command to install from the package manager console (**Tools** > **NuGet Package Manager** > **Package Manager Console**):

  `PM> Install-Package Microsoft.Web.RedisSessionStateProvider`

To install from **Tools** > **NuGet Package Manager** > **Manage NugGet Packages for Solution**, search for `RedisSessionStateProvider`.

For more information see the [NuGet RedisSessionStateProvider page](http://www.nuget.org/packages/Microsoft.Web.RedisSessionStateProvider/ ) and [Configure the cache client](http://azure.microsoft.com/en-us/documentation/articles/cache-dotnet-how-to-use-azure-redis-cache.md#NuGet).

###Modify the Web.Config File
In addition to making assembly references for Cache, the NuGet package adds stub entries in the *web.config* file.

1. Open the *web.config* and find the the **sessionState** element.
1. Enter the values for `host`, `accessKey`, `port` (the SSL port should be 6380), and set `SSL` to `true`. These values can be obtained from the [Azure Portal](http://go.microsoft.com/fwlink/?LinkId=529715) blade for your cache instance.

For more information, see [Connect to the cache](http://azure.microsoft.com/en-us/documentation/articles/cache-dotnet-how-to-use-azure-redis-cache.md#connect-to-cache). Note that the non-SSL port is disabled by default for new caches. For more information about enabling the non-SSL port, see the [Access Ports](https://msdn.microsoft.com/library/azure/dn793612.aspx#AccessPorts) section in the [Configure a cache in Azure Redis Cache](https://msdn.microsoft.com/library/azure/dn793612.aspx) topic. The following markup shows the changes to the *web.config* file.

```XML
<system.web>
  <customErrors mode="Off" />
  <authentication mode="None" />
  <compilation debug="true" targetFramework="4.5" />
  <httpRuntime targetFramework="4.5" />
  <sessionState mode="Custom" customProvider="RedisSessionProvider">
    <providers>  
        <!--<add name="RedisSessionProvider"
          host = "127.0.0.1" [String]
          port = "" [number]
          accessKey = "" [String]
          ssl = "false" [true|false]
          throwOnError = "true" [true|false]
          retryTimeoutInMilliseconds = "0" [number]
          databaseId = "0" [number]
          applicationName = "" [String]
        />-->
       <add name="RedisSessionProvider"
            type="Microsoft.Web.Redis.RedisSessionStateProvider"
            <mark>port="6380"
            host="movie2.redis.cache.windows.net"
            accessKey="m7PNV60CrvKpLqMUxosC3dSe6kx9nQ6jP5del8TmADk="
            ssl="true"</mark> />
    <!--<add name="MySessionStateStore" type="Microsoft.Web.Redis.RedisSessionStateProvider" host="127.0.0.1" accessKey="" ssl="false" />-->
    </providers>
  </sessionState>
</system.web>
```

###Use the Session Object in Code
The final step is to begin using the Session object in your ASP.NET code. You add objects to session state by using the **Session.Add** method. This method uses key-value pairs to store items in the session state cache.

    string strValue = "yourvalue";
	Session.Add("yourkey", strValue);

The following code retrieves this value from session state.

    object objValue = Session["yourkey"];
    if (objValue != null)
       strValue = (string)objValue;

You can also use the Redis Cache to cache objects in your web app. For more info, see [MVC movie app with Azure Redis Cache in 15 minutes](http://azure.microsoft.com/blog/2014/06/05/mvc-movie-app-with-azure-redis-cache-in-15-minutes/).

## Demo 7 - WebJobs

This sample demonstrates creating a WebJob and performing operations with Microsoft Azure WebJobs SDK. The two functions in this example split strings into words (CountAndSplitInWords) and in characters (CharFrequency) and computes their frequencies. The results are stored in Azure Storage Tables.

### Prerequisites

1. Go to http://portal.azure.com and provision a new free Web App.  

 > Note: You can use the Web App you provisioned in the first demo here.

### Demo
1. Open File Explorer and navigate to the Presentation\Web\Demo3 - Web Jobs\source\WebJobs folder in the DevCamp material.
1. Open the project in Visual Studio, enable restore packages from nuget and compile (to download all the packages required inside bin directory)
1. Enter a storage account name and key as instructed in App.config.
1. Right-click project, select " Publish as Azure WebJob.." and then select "run on-demand" from the dropdown.
1. Set a connection string named AzureWebJobsDashboard in the Web App configuration in the preview portal by using the following format.  
  
 > DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY 
1. Find the WebJob under the Web App node in Server Explorer, right-click and select run.
1. Find the storage account in Server Explorer and show the results in queue(textinput) and table(words).
1. Show how to run the WebJob from the Wep App's WebJob setting blade in the portal. Show the log of successful runs.
