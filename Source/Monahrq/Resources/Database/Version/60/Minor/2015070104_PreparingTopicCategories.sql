BEGIN TRY

MERGE INTO TopicCategories target 
USING(Values ('Utilization','Utilization Statistics','These statistics show how often a group of people use health care services in a defined area.',Null,Null,'Topic','False',Null),
('Patient safety',Null,'"Ratings about how safe the hospital is for any type of patient.  Many medical complications can be avoided if patients receive the right care in the hospital.<ul><li>Hospital quality ratings for results of surgical and nonsurgical care are provided.  These results can occur in either surgical or nonsurgical cases.</li><li><u>Recommended Care:</u> Information on how many patients got the care they needed such as the right medicine, surgery, or advice.</li><li><u>Results of care:</u> Information on what happened to patients while being cared for in the hospital or after leaving the hospital. </li></ul>"','Hospital',Null,'Topic','False','"Mistakes happen, and some can be lead to serious harm for patients.  Some hospitals work hard to reduce mistakes and keep patients safe while in the hospital"'),
('Patient survey results',Null,'"Ratings from the Hospital Consumer Assessment of Healthcare Providers and Systems (HCAHPS, pronounced "H-caps").  HCAHPS is a national, standardized survey of hospital patients that asks patients about their experiences during a recent hospital stay."','Hospital',Null,'Topic','False','Some hospitals offer their patients a better experience than others. These types of ratings appear only in the "Patient Experiences" health topic. These ratings are the results of the HCAHPS patient survey'),
('Avoidable Stays Maps',Null,Null,'Hospital',Null,'Topic','False',Null),
('Child health',Null,Null,Null,Null,'Topic','False',Null),
('Childbirth',Null,'Ratings about care for new mothers and newborns.  It includes information about how often and when C-sections and vaginal births are performed. When both the mother and the newborn do not have injuries after childbirth then good results are achieved. Childbirth practice patterns includes quality ratings on how often and when both C-sections and vaginal births are performed.','Hospital',Null,'Condition','False','The best hospitals provide care during labor and delivery that reduces the chance of health problems for new mothers and babies.'),
('COPD(Chronic Obstructive Pulmonary Disease)',Null,'"Ratings about chronic obstructive pulmonary disease, or COPD.  COPD is a lung disease that blocks airflow and makes it hard to breathe"','Hospital',Null,'Topic','False','"Ratings about chronic obstructive pulmonary disease, or COPD.  COPD is a lung disease that blocks airflow and makes it hard to breathe"'),
('Deaths or returns to the hospital',Null,'"Ratings about numbers of deaths or returns to the hospital, also called readmissions.  A high number of deaths or returns to the hospital may mean the hospital is not treating people effectively.A higher number of deaths or returns to the hospital may mean the hospital is not treating people effectively. Measures provide information on the number of deaths and returns to the hospital organized by Health Topic or Condition."','Hospital',Null,'Topic','False','"The best hospitals provide the right care at the right time and in the right way that lowers the number of patients who die in the hospital, die shortly after leaving the hospital, or need to return to the hospital for additional care"'),
('Emergency department (ED)',Null,'Ratings about how quickly patients receive care in the Emergency Department (ED). Reducing the time patients remain in the emergency department can increase the availability of treatment and improve the care that patients receive.','Hospital',Null,'Topic','False','Reducing the time patients wait in the emergency department can improve the care they receive. Some hospitals provide care to patients more quickly than others'),
('Heart attack and chest pain',Null,'Ratings about heart attack care. A heart attack (also called an AMI or an acute myocardial infarction) happens when the arteries leading to the heart become blocked and the blood supply is slowed or stops.','Hospital',Null,'Condition','False','The best hospitals provide patients who have a heart attack or chest pain with the right medicine or procedure at the right time to reduce the chance of the patient getting worse'),
('Heart failure',Null,'Ratings about care for heart failure.  Heart failure or congestive heart failure is a weakening of the heart''s pumping power that prevents the body from getting enough oxygen and nutrients to meet its needs.','Hospital',Null,'Condition','False','"The best hospitals provide patients who have heart failure, which is a condition in which the heart has trouble pumping blood through the body, with the right tests, medicine, and information to make sure the heart pumps blood properly"'),
('Heart surgeries and procedures',Null,'Ratings about surgeries and procedures related to the heart such as angioplasty and coronary bypass surgery.','Hospital',Null,'Condition','False','The best hospitals use the right procedures during surgery to reduce the chance of complications'),
('Hip or knee replacement surgery',Null,'"Ratings about surgery to replace the hip or knee joint in people who have severe, on-going joint pain that makes it difficult to carry on with normal daily activities. Total joint (hip or knee) replacement is a surgery to replace the ends of both bones in a damaged joint to create new joint surfaces."','Hospital',Null,'Topic','False','"Ratings about surgery to replace the hip or knee joint in people who have severe, on-going joint pain that makes it difficult to carry on with normal daily activities. Total joint (hip or knee) replacement is a surgery to replace the ends of both bones in"'),
('Imaging',Null,'"Ratings about imaging procedures including magnetic resonance imaging (MRI), mammography, and computerized axial tomography (CAT scans)."','Hospital',Null,'Topic','False','"The best hospitals only use imaging, such as MRIs, CT scans, and EKG tests, when it is necessary"'),
('Infections',Null,'"Ratings about health care associated infections, or HAIs. Health care-associated infections  are infections that people acquire while they are receiving treatment for another condition in a health care setting. HAIs can be acquired anywhere health care is delivered, including  hospitals, clinics, or nursing homes. HAIs may be caused by a bacteria or virus."','Hospital',Null,'Topic','False','"The best hospitals prevent their patients from getting infections while in the hospital, also known as heath care-associated infections"'),
('Nursing care',Null,'"Ratings about nursing sensitive care. Nurses and the care they provide exert a great deal of influence over healthcare quality, patient safety, and patient outcomes."','Hospital',Null,'Topic','False','Nurses are very important to getting good quality care.  They watch to make sure the patient is responding well to care and take steps to prevent many complications and problems.'),
('Other surgeries',Null,'Ratings about surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery.Brain surgery (craniotomy) and hip replacement are examples of other surgeries.','Hospital',Null,'Topic','False','Ratings about surgeries other than heart surgery such as brain surgery (craniotomy) and hip replacement surgery.Brain surgery (craniotomy) and hip replacement are examples of other surgeries.'),
('Pneumonia',Null,'"Ratings about pneumonia care.  Pneumonia is a serious lung infection that can cause difficulty breathing, fever, cough, and fatigue."','Hospital',Null,'Condition','False','The best hospitals prevent patients from getting pneumonia or provide effective care  for a patient with pneumonia'),
('Prevention and Treatment',Null,'Ratings about serious medical conditions that could have been prevented if patients are provided the right care at the right time.','Hospital',Null,'Topic','False','Ratings about serious medical conditions that could have been prevented if patients are provided the right care at the right time.'),
('Stroke',Null,'"Ratings about stroke care.  A stroke happens when the blood supply to the brain stops. This topic includes carotid endarterectomy surgery, an operation intended to prevent stroke."','Hospital',Null,'Condition','False','The best hospitals provide high-quality and timely care to patients who have experienced a stroke'),
('Summary Scores',Null,'Measures that combine more than one measure into one score.  Composite measures provide a summary of quality or performance.','Hospital',Null,'Topic','False','"The best hospitals are high-quality, which means they do a good job performing surgeries and treating patients’ health conditions. This summary score combines the hospital ratings for major surgeries, health conditions, and patient safety"'),
('Surgical patient safety',Null,'"Ratings about how safe the hospital is for patients having surgery. Many medical complications can be avoided if patients receive the right care before, during, and after surgery.<ul><li>Hospital quality ratings for recommended care before surgery and after surgery, and results of surgical care.</li><li><u>Recommended care before surgery:</u> Information on how many patients got the care they needed such as the right medicine, surgery, or advice.</li><li><u>Recommended care after surgery:</u> Information on how many patients got the care they needed such as the right medicine, surgery, or advice after a surgery.</li></ul>"','Hospital',Null,'Topic','False','"The best hospitals provide care before, during, and after surgery to reduce the number of errors and complications a patient could experience"'),
('Women health',Null,Null,Null,Null,'Topic','False',Null),
('Nursing Home',Null,Null,'NursingHome',Null,'Topic','False',Null),
('Health Care Cost and Quality',Null,'Displays the number of treated patients and the average cost to a hospital for a procedure alongside the hospital’s quality ratings','Hospital',Null,'Topic','False','Displays the number of treated patients and the average cost to a hospital for a procedure alongside the hospital’s quality ratings'),
('General Topic 1','General Topic 1','General Topic 1','Hospital','HCUP County Hospital Stays Data','Topic','False',Null),
('General Topic 2','General Topic 2','General Topic 2','Hospital','HCUP County Hospital Stays Data','Topic','False',Null)) 
as source (Name,LongTitle,Description,TopicType,WingTargetName,CategoryType,IsUserCreated,ConsumerDescription) on source.Name = target.Name
WHEN MATCHED THEN UPDATE
	SET target.Name = source.Name,
		target.LongTitle = source.LongTitle,
		target.Description = Replace(source.Description, '"',''),
		target.TopicType = source.TopicType,
		target.WingTargetName = source.WingTargetName,
		target.CategoryType = source.CategoryType,
		target.IsUserCreated = source.IsUserCreated,
		target.ConsumerDescription = Replace(source.ConsumerDescription, '"','') 
WHEN NOT MATCHED THEN 
	INSERT (Name,LongTitle,Description,TopicType,WingTargetName,CategoryType,IsUserCreated,ConsumerDescription) 
	VALUES(source.Name,source.LongTitle,Replace(source.Description, '"',''),source.TopicType,source.WingTargetName,source.CategoryType,source.IsUserCreated,Replace(source.ConsumerDescription, '"','') );


	update			tc
	set				tc.Name = 'COPD (Chronic Obstructive Pulmonary Disease)'
	from			TopicCategories tc
	where			tc.Name = 'COPD(Chronic Obstructive Pulmonary Disease)'

END TRY
BEGIN CATCH
DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 

END CATCH