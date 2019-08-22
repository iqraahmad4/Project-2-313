module part4b(SW, HEX0);
	part4(SW, HEX0);
	
	input [2:0]SW;
	output [0:6]HEX0;
	
	assign HEX0[0]=SW[0];
	assign HEX0[1]=(~(SW[0]&SW[1]))|(SW[0]&SW[1]);
	assign HEX0[2]=(~(SW[0]&SW[1]))|(SW[0]&SW[1]);
	assign HEX0[3]=SW[1]|SW[0];
	assign HEX0[4]=1;
	assign HEX0[5]=1;
	assign HEX0[6]=~SW[1];
		
endmodule
