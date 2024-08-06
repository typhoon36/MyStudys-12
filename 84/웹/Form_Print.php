<meta http-equiv="Content-Type" content="text/html; charset=utf8" />

<?
	$Id = $_POST["name"];
	$Pass = $_POST["password"];
	$Gen = $_POST["gender"];
	$Eng = $_POST["part_eng"];
	$Math =$_POST["part_math"]; 

	echo "아이디 : $Id<br>";
	echo "비밀번호 : $Pass<br>";
	echo "성별 : $Gen<br>";
	echo "영어 : $Eng<br>";
	echo "수학 : $Math<br>";
?>