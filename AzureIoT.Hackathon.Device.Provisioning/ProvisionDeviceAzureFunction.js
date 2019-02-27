module.exports = async function (context, req) {
    // Decode registration id
    // stringify req and context data
    // Initialize twin based on regId
    // iotHubHostName and initialTwin
    context.log(req.body);
    context.log("\nDELIM\n");
    context.log(req.body.deviceRuntimeContext.registrationId);
    context.log("\nDELIM\n");
    context.log(req.body.linkedHubs[0]);

    var registrationId = req.body.deviceRuntimeContext.registrationId;

    var first = registrationId.indexOf("-",0);
    var second = registrationId.indexOf("-",first+1);
    var third = registrationId.indexOf("-",second+1);
    var fourth = registrationId.indexOf("-",third+1);
    
    var manufacturer = registrationId.substring(first+1,second);
    var model = registrationId.substring(second+1,third);

    var majorVersion = registrationId.substring(third+1, fourth);
    var minorVersion = registrationId.substring(fourth+1);

    var url = "https://example.com/fw/".concat(majorVersion,".",minorVersion);
    var myres = {
            status: 200,
            body: 
            {
                "iotHubHostName": req.body.linkedHubs[0],
                "initialTwin": 
                {
                    "tags": 
                    {
                        "manufacturer" : manufacturer ,
                        "model" : model,
                        "date" : "02-27-2019"
                    },
                    "properties": 
                    {
                        "desired": 
                        {
                            "firmware" : 
                            {
                                "version" : 
                                {
                                    "major" : parseInt(majorVersion),
                                    "minor" : parseInt(minorVersion)
                                },
                            "url" : url
                            }
                        }
                    }
                }
            }
    };

    context.log(JSON.stringify(myres));

    context.res = myres;
    
    return;
}