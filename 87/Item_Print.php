<?php

$ItName = $_POST["ItemName"];
$ItLevel = $_POST["ItemLevel"];
$ItAttRate = $_POST["ItemAttRate"];
$ItPrice = $_POST["ItemPrice"];

//echo "아이템 이름 : " . $ItName . "<br>";
//echo "아이템 레벨 : " . $ItLevel . "<br>";
//echo "아이템 공격상승률 : " . $ItAttRate . "<br>";
//echo "아이템 가격 : " . $ItPrice . "<br>";

$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");

if (!$con) 
{
    die("Could not connect: " . mysqli_connect_error());
}

if ($ItName == "" || $ItName == "guest") {
    echo "다시 입력해주세요. guest 는 리스트만 출력합니다.<br><br>";
}

if ($ItName != "" && $ItName != "guest") 
{
    $check = mysqli_query($con, "SELECT ItemName FROM ItmeGrades WHERE ItemName = '{$ItName}'");

    $numrows = mysqli_num_rows($check);

    if ($numrows == 0) 
    {
        $Result = mysqli_query($con, "INSERT INTO ItmeGrades (ItemName, ItemLevel, ItemAttRate, ItemPrice)
                                    VALUES ('{$ItName}', '{$ItLevel}', '{$ItAttRate}', '{$ItPrice}')");

            if ($Result)
            	echo "추가성공!<br>";
			else
				echo ("Create error .\n");
    }
    else 
    {
            echo "이미 존재하는 아이템 이름은 리스트만 출력합니다.<br><br>";
    }


  
}   

    echo '<form method= "post " action="Item_Input.html">';
    echo '<input type="submit" value="이전 페이지로 돌아가기">';
    echo '</form>';


    echo'<form name = "Item_List" method= "Post">';

    //## 100개씩 가져오기
    $LowList = mysqli_query($con, "SELECT * FROM ItmeGrades ORDER BY idx DESC LIMIT 0, 10");     
    if(!$LowList)
            echo("쿼리 오류 발생 :" . mysqli_error($con));

    
    echo '<table border="1" width="710" cellspacing="0" cellpadding="4">';
    echo '<tr align="center" bgcolor="blue">';
             echo '<td style="color:white">선택</td>';
             echo '<td style="color:white">고유번호</td>';
             echo '<td style="width=170px;color:white">아이템이름</td>';
             echo '<td style="color:white">레벨</td>';
             echo '<td style="color:white">공격상승률</td>';
             echo '<td style="color:white">가격</td>';


    $rowsCount = mysqli_num_rows($LowList);
    for($a_idx= 0; $a_idx< $rowsCount; $a_idx++)
    {

        $a_row = mysqli_fetch_array($LowList);
        if($a_row != false)
        {
          
             $UniqueId = $a_row['idx'];

 			echo '<tr align="center">';
				echo '<td>';
				echo '<input type="radio" name="DelItName" value="'.$UniqueId.'">';			
				echo '</td>';
				echo '<td>';
				echo $a_row['idx'];
				echo '</td>';
				echo '<td>';
				echo $a_row['ItemName'];
				echo '</td>';
				echo '<td>';
				echo $a_row['ItemLevel'];
				echo '</td>';
				echo '<td>';
				echo $a_row['ItemAttRate'];
				echo '</td>';
				echo '</td>';
				echo '<td>';
				echo $a_row['ItemPrice'];
				echo '</td>';
			echo '</tr>';
        
        }
    }

    echo '</table>';

	echo '<br><br><input type="submit" value="이전페이지로 돌아가기..." formaction="Item_Input.html">&nbsp;&nbsp;&nbsp;&nbsp; ' ;
	echo '<input type="submit" value="선택아이템삭제" formaction="Item_Del.php">';
	echo '</form>';

	mysqli_close($con);
 


?>