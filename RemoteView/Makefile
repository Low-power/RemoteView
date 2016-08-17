MCS := gmcs
ifndef MONO_PATH
export MONO_PATH = /usr/lib/mono/2.0
endif
LIBS = -lib:$(MONO_PATH) -r:System.Windows.Forms.dll -r:System.Drawing.dll

RemoteView.exe:
	$(MCS) -recurse:\*.cs -out:$@ $(LIBS)

clean:
	rm -f RemoteView.exe
