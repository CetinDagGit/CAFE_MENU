This project is about a cafe menu. 
MVC pattern was used as a design pattern. 
MSSQL was used as a database. 
The project consists of two layouts, Home (User) and Admin. 
Admin login is directed with a simple separation depending on the user. 
HASHPASSWORD and SALTPASSWORD methods were used for encryption.

Home: The Home section has been developed for end users-customers.
Screens have been designed where products entered by the admin can be displayed, a basket can be created and an order can be created.
The display of products has been paged, and Redis has been used for cashing.
While the price information is in Turkish Lira, the foreign currency (Dollar) equivalent is instantly drawn from the Central Bank page, calculated and automatically written next to it.

Admin: In the Admin section, there are Users, Products, Categories and Features menus.
These menus include List-Add-Update functions and pages.
Pagination and Redis cashing have been implemented in the Products list.
Price information is displayed with the same function on the home page.

Dashbard: The main menu (default opening screen) of the admin layout is in the dashboard feature.
There is a pie slice display of the products in the system divided by category.
There is also total earnings information for daily sales, updated every 10 seconds.
The Visual Studio Copilot feature was used during the development of this page.
