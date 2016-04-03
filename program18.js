var express=require("express");
var app=express();
app.get(process.argv[3], function(req,resp){
    var obj={};

    for(var key in req.query)
    {
        obj[key]=req.query[key];
    }
    obj['results']='recent';
   resp.send(obj); 
});

app.listen(process.argv[2]);