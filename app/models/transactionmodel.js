var mongoose = require('mongoose');

module.exports = mongoose.model('Transcations', {		
	lender : [{
		lenderId : {type : Integer},
		lenderName : {type : String , default : ""},
		amountGiven : {type : Double}
	}],
	Borrower : [{
		borrowerId : {type : Integer},
		borrowerName : {type : String , default : ""},
		amountReceived : {type : Double}
	}],
	status : {type : Integer, default: 0}
});