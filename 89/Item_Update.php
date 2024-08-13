<?php

   // 데이터베이스 연결
    $con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
    if(!$con) 
	{
        		die("Could not Connect: " . mysqli_connect_error());
    	}


    	// POST된 데이터를 처리.
    	foreach ($_POST as $key => $value) {
      	  if (strpos($key, 'ItemName_') === 0 || strpos($key, 'ItemLevel_') === 0 || strpos($key, 'ItemAttRate_') === 0 || strpos($key, 'ItemPrice_') === 0) 
		{
            		$UniqueId = explode('_', $key)[1];
            
            // 항목별로 나누어 업데이트합니다.
            if (strpos($key, 'ItemName_') === 0) {
                $ItemName = mysqli_real_escape_string($con, $value);
                $query = "UPDATE ItemGrades SET ItemName='{$ItemName}' WHERE idx='{$UniqueId}'";
            } elseif (strpos($key, 'ItemLevel_') === 0) {
                $ItemLevel = mysqli_real_escape_string($con, $value);
                $query = "UPDATE ItemGrades SET ItemLevel='{$ItemLevel}' WHERE idx='{$UniqueId}'";
            } elseif (strpos($key, 'ItemAttRate_') === 0) {
                $ItemAttRate = mysqli_real_escape_string($con, $value);
                $query = "UPDATE ItemGrades SET ItemAttRate='{$ItemAttRate}' WHERE idx='{$UniqueId}'";
            } elseif (strpos($key, 'ItemPrice_') === 0) {
                $ItemPrice = mysqli_real_escape_string($con, $value);
                $query = "UPDATE ItemGrades SET ItemPrice='{$ItemPrice}' WHERE idx='{$UniqueId}'";
            }

            // 쿼리 실행
            if (!mysqli_query($con, $query)) {
                echo "Error updating record: " . mysqli_error($con) . "<br>";
            }
        }
    }




   $data = [];
   $JsonMsg = "";
   $result = mysqli_query($con, "SELECT * FROM ItmeGrades ORDER BY idx DESC LIMIT 0, 100");
   if ($result) 
   {
   	while ($row = mysqli_fetch_assoc($result)) 
	{
		$data[] = $row;
	}
            
        	$json_data = json_encode(['itemArr' => $data], JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);

	$file_path = 'item_data.json';

	if (file_put_contents($file_path, $json_data)) 
	{
		$JsonMsg = "JSON 파일이 생성되었습니다. $file_path";  
 	} 
	else 
	{
		$JsonMsg = "JSON 파일 생성 오류!!";  
            }
   } 
   else 
   {
	$JsonMsg = "데이터를 가져오는 데 오류가 발생했습니다.";  
   }

    //#테이블을 Json 파일로 저장 

    //  JavaScript      
    echo '<form id="postForm" method="post" action="' . htmlspecialchars($_SERVER['HTTP_REFERER']) . '">';    //$_SERVER['HTTP_REFERER'] // 이전 페이지 주소
    echo '<input type="hidden" name="update_message" value="테이블이 성공적으로 수정되었습니다.">';
    echo '<input type="hidden" name="json_message" value="' . htmlspecialchars($JsonMsg) . '">';
    echo '</form>';
    echo '<script type="text/javascript">document.getElementById("postForm").submit();</script>';

    // 데이터베이스 연결 종료
    mysqli_close($con);
    exit();
?>