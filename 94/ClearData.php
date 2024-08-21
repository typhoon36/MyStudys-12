<?php
	$u_id = $_POST["Input_user"];

	if( empty($u_id) )
		die("Input_user is emplty.");

	$con = mysqli_connect("Localhost", "DB_Id", "DB_Pw", "DB_Name");
	//"Localhost" <-- 같은 서버 내에 있는 DB를 찾겠다는 의미

	if( ! $con )
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query( $con, "SELECT user_id FROM KDB_1 WHERE user_id= '{$u_id}' " );

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{
		//mysqli_num_rows() 함수는 데이터베이스에서 쿼리를 보내서 나온 행의 개수를
		//알아낼 때 쓰임 즉 0 이라는 뜻은 해당 조건을 못 찾았다는 뜻
		die( "Id does not exist." );
	}

	// 배열 생성

	$Jdata = array( "SkList" => array(1, 1, 1) );

	// 배열을 JSON 문자열로 변환

	$jsonString = json_encode($Jdata);

	mysqli_query($con, "UPDATE KDB_1 SET best_score = 0, game_gold = 0, floor_info = '', info = '' '{$jsonString}'  WHERE user_id = '{$u_id}' ");

	
	
	echo "ClearDataSuccess~";

	mysqli_close($con);
?> 