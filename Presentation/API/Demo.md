# API App Demos

## Demo 1 - Create an API App

This is a quick demo showing how quickly you can create a new API App using Visual Studio.

1. Create a new **ASP.NET Web Application** project using Visual Studio with the name **ProductsApp**
2. Select the **Azure API App (Preview)** project template
3. Add a Contact class to the project with the name **Product.cs**
4. Add the following code to the contact class:

		namespace ProductsApp.Models
		{
		    public class Product
		    {
		        public int Id { get; set; }
		        public string Name { get; set; }
		        public string Category { get; set; }
		        public decimal Price { get; set; }
		    }
		}

5. Delete the **ValuesController.cs** file in the **Controllers** folder.
6. Add a new Empty Controller to the Web API project using the Default Scaffolding and the name **ProductsController.cs**
7. Add the following code to the controller

		using ProductsApp.Models;
		using System;
		using System.Collections.Generic;
		using System.Linq;
		using System.Net;
		using System.Web.Http;

		namespace ProductsApp.Controllers
		{
		    public class ProductsController : ApiController
		    {
						List<Product> products = new List<Product>
		        {
		            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 },
		            new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M },
		            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M }
		        };

		        public IEnumerable<Product> GetAllProducts()
		        {
		            return products;
		        }

		        public IHttpActionResult GetProduct(int id)
		        {
		            var product = products.FirstOrDefault((p) => p.Id == id);
		            if (product == null)
		            {
		                return NotFound();
		            }
		            return Ok(product);
		        }
		    }
		}

8. Deploy the application to a new API App instance in Azure

	> Ensure that the Access Level is set to **Available to Everyone**

9. Open **Internet Explorer** and navigate to **https://www.hurl.it/**
10. In the **Destination** textbox, add the URL for your API app with the relative url **/api/Products** appended to the end

	> Ensure that you use the https scheme. `https://[API App Name].azurewebsites.net/api/Products`

11. Click **Launch Request**
12. Leave the project in Visual Studio open for future demos

## Demo 2: Connecting to an API App

This demo	shows how to use a desktop client to connect to an API App instance.

> This demo requires you to complete Demo 1 first. The following demo requires Postman (Chrome Standalone Extension).  You can use other tools such as Fiddler.

1. Open the Postman client
2. Click the **Import** button at the top of the application
3. Select the **Download from link** button and add the URL to your API app with the relative url **/swagger/docs/v1** appended to the end

	> Explain here that the application is downloading the Swagger metadata and using this to determine valid endpoints that you can call.  This endpoint is also versioned. Explain that the URL was specified in your **SwaggerConfig.cs** file.  `https://[API App Name].azurewebsites.net/swagger/docs/v1`

4. Close the **Import** dialog
5. Click the **Collections** tab and view the calls available for the **ProductsApp** API
6. Select the first **GET** operation and then click the **Send** button

	> Early versions of Postman will incorrectly use the [http] scheme when the [https] scheme is required.  If you run into this issue, simply make sure that your request is using the [https] scheme.

7. Delete the **ProductsApp** API in the **Collections** tab
8. Return to Visual Studio and open the **ProductsController.cs** file in the **Controllers** folder
9. Add the following method to the **ProductsController** class

		public IHttpActionResult AddProduct(Product product)
		{
				if (product == null)
				{
						return BadRequest();
				}
				product.Id = products.Count + 1;
				products.Add(product);
				return Created(
						Url.Link("DefaultApi", new {controller = "Products", id = product.Id}),
						product
				);
		}

10. Deploy the application to the same API App instance in Azure
11. Return to Postman and Import the API using the **Import** dialog again
12. Expand the new **ProductsApp** API in the **Collections** tab and observe that there are now 3 possible operations
13. Click the only **POST** operation
14. Select the **raw** option and **JSON (application/json)** content type for the body of the **POST** operation.
15. Use the following content for the body.

		{
				"Name": "String Cheese",
				"Category": "Groceries",
				"Price": 0.50
		}

## Demo 3: Viewing the Metadata for an API App

	>  This demo requires you to complete Demo 1 first.

1. Browse to [Preview Portal](https://portal.azure.com)
2. Browse to the previously created API App instance
3. Expand the **Essentials** panel and then click the **All Settings** link
4. Click the **Application Settings** option to observe the *Access Level* value
5. Return to the **ProductsApp** blade and click the **API Definition** tile
6. Click the **Download Swagger** button
7. View the downloaded JSON file
