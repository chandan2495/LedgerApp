var fs=require("fs");
var path=require("path");
module.exports = function(dirname, ext, callbackfunction){
    fs.readdir(dirname,function(err,list){
        if (err)
            return callbackfunction(err) // early return             
        var ext1='.'+ext;
        var output=[];
       for(var i=0;i<list.length;i++)
       {
            if(path.extname(list[i])==ext1){
                output[i]=list[i];
              }
       }
       // for(var i=0;i<output.length;i++)
       //     console.log(output[i]);
       if (typeof callbackfunction == 'function')
        callbackfunction.call(err,output);
  });
 }

