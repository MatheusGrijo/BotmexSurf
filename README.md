# BitBotBackToTheFuture
Bot of Bitmax

Example file "key.txt" (Configuration)

{

"key":"",

"secret":"",

"domain":"https://testnet.bitmex.com",

"pair":"XBTUSD",

"contract":300,

"profit":0.2,

"fee":0.075,

"long":"enable",

"short":"enable",

"interval":2000,

"intervalOrder":60000,

"timeGraph":"1m",


"webserver":"enable",

"webserverConfig":"http://localhost:5321/bot/",

"webserverIntervalCapture":300000,

"webserverKey":"",

"webserverSecret":"",


"indicatorsEntry":[

	{
		
"name":"CCI",

		"period":8
	},
	{
		
"name":"RSI",

		"period":8
	},
	{
		
"name":"BBANDS",

		"period":7
	}	
],



"indicatorsEntryCross":[


		{
			
"name":"MA",

			"period":5
		}


],

"indicatorsEntryDecision":[


	{
		
"name":"RSI",

		"period":8,
		"decision":"enable",
		"decisionPoint":40,
		"tendency":"enable"
	}
	

	]


}


DONATE

BTC 39DWjHHGXJh9q82ZrxkA8fiZoE37wL8jgh

BCH qqzwkd4klrfafwvl7ru7p7wpyt5z3sjk6y909xq0qk

ETH 0x3017E79f460023435ccD285Ff30Bd10834D20777

ETC 0x088E7E67af94293DB55D61c7B55E2B098d2258D9

LTC MVT8fxU4WBzdfH5XgvRPWkp7pE4UyzG9G5
