var mymodule=require("./newmodule.js");

mymodule(process.argv[2],process.argv[3],function(data,err){
   // if(err)
   //    console.log(err);
   // else{
   for(var i=0;i<data.length;i++)
      console.log(data[i]);
   // }
});
