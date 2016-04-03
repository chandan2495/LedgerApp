var mongoose = require('mongoose');

module.exports = mongoose.model('Users', {
	displayName : {type : String, default: ''},
	userName : {type : String, default: ''},
	owe : [{
		transactionId : {type : Integer}
	}],
	lend : [{
		transactionId : {type : Integer}
	}]
});