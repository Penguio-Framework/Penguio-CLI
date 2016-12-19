 
http://stackoverflow.com/questions/7264682/running-msbuild-programmatically


cli tool 
	
	ms platform add android
	ms platform add windows
		copys the library into the platforms/windows folder 
	ms build windows
		does diff on assets 
		updates content.mgcb
		copys game files from csproj 
		runs msbuild
	ms run windows
		runs build and then executes exe or adb or httpserver
	


	mm build android
		generates output
	mm run android
		generates and runs output
	mm debug android
		opens visual studio?
	mm update content
		reads from content directory and updates content idk
	mm generate font font.xml
		font.xml defines all the fonts you need
		 "BabyDoll", 30, new[] { 36 }, "x"
	mm generate splash splash.xml
	mm create 

move font builder into cleaner place
decide on assetmanager
get fonts working
get sounds working
json config file
get icons working
proper engine nuget
get splashscreen working
generate splash screen
get web working
get ios working
	https://forums.xamarin.com/discussion/3696/how-to-build-from-msbuild-command-line-trig-build-on-osx-from-windows
