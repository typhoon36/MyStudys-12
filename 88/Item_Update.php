<?php

	// DB 연결
	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
	if (!$con) {
   	 die('Could not Connect: ' . mysqli_connect_error());
	}

	// POST 된 데이터 처리
	foreach($_POST as $key => $value) {
    	// $key가 아이템 이름 또는 레벨 또는 아이템 공격 상승률 또는 아이템 가격으로 시작하는 경우
    	if (strpos($key, 'ItemName_') === 0 || 
       	 strpos($key, 'ItemLevel_') === 0 || 
      	  strpos($key, 'ItemAttRate_') === 0 || 
        	strpos($key, 'ItemPrice_') === 0) {
        
       	 // 고유번호 추출
        	$UniqueId = explode('_', $key)[1];

       	 // 출력
        	if (strpos($key, 'ItemName_') === 0) {
            echo "Test : " . $key . ":" . $UniqueId . "<br>";
        }
    }
}

// 데이터베이스 연결 종료
mysqli_close($con);    

?>
