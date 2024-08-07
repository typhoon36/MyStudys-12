<?
$Id = $_POST['user_id'] ?? '';
$Name = $_POST['user_name'] ?? '';
$PW = $_POST['password'] ?? '';
$PWRe = $_POST['password_check'] ?? '';
$Se = $_POST['gender'] ?? '';

// 전화번호를 조합하여 원하는 형식으로 출력
$phone_country_code = $_POST['phone_country_code'] ?? '';
$phone_1 = $_POST['phone_1'] ?? '';
$phone_2 = $_POST['phone_2'] ?? '';
$Phonenum = "$phone_country_code-$phone_1-$phone_2";

// 주소의 필드 이름 수정
$Ad = $_POST['address'] ?? '';

// 취미 체크박스는 isset을 사용하여 확인
$HobbyMovie = isset($_POST['hobby_movie']) ? 'Yes' : 'No';
$HobbyRead = isset($_POST['hobby_read']) ? 'Yes' : 'No';
$HobbyShopping = isset($_POST['hobby_shopping']) ? 'Yes' : 'No';
$HobbyExercise = isset($_POST['hobby_exercise']) ? 'Yes' : 'No';

$Intro = $_POST['introducing'] ?? '';

$form_title = $_POST['form_title'] ?? '';



    echo "아이디 : $Id<br>";
    echo "이름 : $Name<br>";
    echo "비밀번호 : $PW<br>";
    echo "비밀번호 확인 : $PWRe<br>";
    echo "성별 : $Se<br>";
    echo "전화번호 : $Phonenum<br>";
   echo "주소 : $Ad<br>";
    echo "영화감상 :  $HobbyMovie<br>";
    echo "독서 : $HobbyRead<br>";
   echo "쇼핑 :  $HobbyShopping<br>";
   echo "운동 : $HobbyExercise <br>";
   echo "자기소개 :  $Intro<br>"; 
    



   echo "제목 : $form_title<br>";		
?>

