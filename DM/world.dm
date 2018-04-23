/world
	name = "ByondLang"
	mob = /mob

	New()
		world.Export("http://127.0.0.1:1945/action=clear");

/mob
	step_size = 32;
	var/program = "";
	verb/new_program(var/code as text)
		var/http[] = world.Export("http://127.0.0.1:1945/action=new_program&code=[url_encode(code)]");

		if(!http)
			usr << "Failed to connect."
			return

		program = file2text(http["CONTENT"])
		usr << "Launched program [program]"

	verb/run_cycles(var/amount as text)
		var/http[] = world.Export("http://127.0.0.1:1945/action=execute&id=[program]&cycles=[amount]");

		if(!http)
			usr << "Failed to connect."
			return

		usr << "Cycled... [file2text(http["CONTENT"])]"

	verb/get_buffered()
		var/http[] = world.Export("http://127.0.0.1:1945/action=get_buffered&id=[program]");

		if(!http)
			usr << "Failed to connect."
			return

		usr << "[file2text(http["CONTENT"])]"