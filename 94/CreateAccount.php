<?php
	$u_id = $_POST["Input_id"];
	$u_pw = $_POST["Input_pw"];
	$nick = $_POST["Input_nick"];

	if( empty($u_id) )
		die("u_id is empty.\n");

	if(empty($u_pw))
		die("u_pw is empty.\n");

	if(empty($nick))
		die("nick is empty.\n");

	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");

	if( !$con)
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query($con, "SELECT user_id FROM KDB_1 WHERE user_id = '{$u_id}' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{	// 즉 0이 아니라는 뜻은 내가 생성하려고 하는 ID 값이 이미 존재 한다는 뜻이다.
		// (누군가 이미 사용하고 있다는 뜻)
		die("ID does exist.");
	}

	$check = mysqli_query($con, "SELECT nick_name FROM KDB_1 WHERE nick_name = '{$nick}' ");
	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{	// 즉 0이 아니라는 뜻은 내가 생성하려고 하는 NickName 값이 이미 존재 한다는 뜻이다.
		// (누군가 이미 사용하고 있다는 뜻)
		die("Nickname does exist.");
	}

	// 배열 생성

	$Jdata = array( "SkList" => array(1, 1, 1) );

	// 배열을 JSON 문자열로 변환

	$jsonString = json_encode($Jdata);


	$Result = mysqli_query($con, "INSERT INTO KDB_1 (user_id, user_pw, nick_name) VALUES 
				( '{$u_id}', '{$u_pw}', '{$nick}', '{$jsonString}'  );" );

	if($Result)
		echo "Create Success.";
	else 
		echo "Create error.";

	mysqli_close($con);

?> 