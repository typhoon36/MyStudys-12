<?php
	$u_id=$_POST["Input_id"];
	$u_pw=$_POST["Input_pw"];

	//echo"$u_id<br>";
	//echo"$u_pw<br>";

	$con= mysqli_connect("Localhost", "typhoon", "jun3824!" , "typhoon");

	if(!$con)
		die("Couldnot connect" . mysqli_connect_error() );

	
	$check = mysqli_query($con, "SELECT * FROM KDB_1 WHERE user_id='{$u_id}' ");
	
	$numrows = mysqli_num_rows($check); //데이터베이스에서 쿼리를 보내고 나온 행의 갯수
	if($numrows == 0)//0이면 die안의 메시지 출력
	{
		
		//like return
		die("Id does not exitst.");
	}
	
	//echo"DB 접속 성공&유저정보 찾기 성공";
	
	$row = mysqli_fetch_assoc($check);
	if($row)
	{
		if( $u_pw == $row["user_pw"] )
		{
			echo "아이디 : " . $row["user_id"] . "<br>";
			echo "별명 : " .$row["nick_name"] . "<br>";
			echo "점수 : " . $row["best_score"] . "<br>";
			echo "게임머니 : " . $row["game_gold"] . "<br>";
		}
		else
		{
			die("Password does not Match.");
		}
	}

	mysqli_close($con);

?>