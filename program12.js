var http=require("http");
var url=require("url");

    var obj=url.parse(process.argv[2],true);
    var count=0;
    var jsonResp;
    console.log(obj.pathname);

    if(obj.pathname=="/api/unixtime")
    {
        var date=new Date(obj.query.iso);
        jsonResp={
          'unixtime' : date.getTime()  
        };
    }
    else if((obj.pathname=="/api/parsetime")){
        var date=new Date(obj.query.iso);
        var jsonResp={
          "hour" : date.getHours(),
          "minute": date.getMinutes(),
          "second" : date.getSeconds()
        }; 
    }
    console.log(jsonResp);