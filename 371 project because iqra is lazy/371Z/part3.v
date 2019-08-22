module part3(SW, LEDR, LEDG);
	part1(SW,LEDR);
	
	input [17:0]SW;
	output [17:0]LEDR;
	output [17:0]LEDG;
	
	wire [2:0]U;
	wire [2:0]V;
	wire [2:0]W;
	wire [2:0]X;
	wire [2:0]Y;
	wire [2:0]s;
	
	assign U = SW[2:0];
	assign V = SW[5:3];
	
	assign W = SW[8:6];
	assign X = SW[11:9];
	
	assign Y = SW[14:12];
	
	assign s = SW[17:15];
	assign LEDG = (~s&X)|(s&Y);
endmodule