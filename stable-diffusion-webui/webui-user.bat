@echo off

set PYTHON=
set GIT=
set VENV_DIR=
set COMMANDLINE_ARGS= --api
::set COMMANDLINE_ARGS=--api
::set COMMANDLINE_ARGS=--disable-nan-check
::set COMMANDLINE_ARGS=--no-half

call webui.bat
