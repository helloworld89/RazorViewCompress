#RazorViewCompress
  
  Use to remove white spaces in razor. Work With MVC3 above

##How to use it
Insert codes below in Application_Start method in Global.asax file:

    RazorViewCompress.CompressConfig.Config(true, true, true);

Each bool with different setting,you can adjust it as you like. 

Then view the source code in the browser, you will find page compressed.

## Note:
  the default Layout in _ViewStart.cshtml shouldn't be changed, or this can't find the layout page.
