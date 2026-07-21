Function main
	' Start error handling tasks
	Xqt robot_errorHandling, NoEmgAbort
	Xqt robot_estopHandling, NoEmgAbort
	
	' Init robots
	Call robot1_init
	Call robot2_init
	Call robot3_init
Fend

