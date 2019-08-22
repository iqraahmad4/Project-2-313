/*assign m[0] = (~s&x[0])|(s&y[0]);
assign m[1] = (~s&x[1])|(s&y[1]);
assign m[2] = (~s&x[2])|(s&y[2]);
assign m[3] = (~s&x[3])|(s&y[3]);
assign m[4] = (~s&x[4])|(s&y[4]);
assign m[5] = (~s&x[5])|(s&y[5]);
assign m[6] = (~s&x[6])|(s&y[6]);
assign m[7] = (~s&x[7])|(s&y[7]);*/


module part2 (SW, LEDR, LEDG);
	part1(SW,LEDR);
	
	input [17:0]SW;
	output [17:0]LEDR;
	output [17:0]LEDG;
	
	wire [7:0]X;
	wire [7:0]Y;
	wire [7:0]s;
	
	assign X = SW[7:0];
	assign Y = SW[15:8];
	assign s = {8{SW[17]}};
	assign LEDG = (~s&X)|(s&Y);

endmodule
