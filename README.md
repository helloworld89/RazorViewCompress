#RazorViewCompress
  
##Functions
* Remove white spaces in original razor file.
* Remove white spaces in genetated html file.
* Zip the genetated html

##How to use it
Insert codes below in Application_Start method in Global.asax file:

    RazorViewCompress.CompressConfig.Config(true, true, true);

Each bool with different setting,you can adjust it as you like. 

Then view the source code in the browser, you will find page compressed.

## Note:
* Work With MVC3 above
* The default Layout name shouldn't be changed, or this can't find the layout page.
