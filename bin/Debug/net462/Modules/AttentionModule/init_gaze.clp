;--------------------------------------------------------TEMPLATES FOR SUBJECT, SURROUNDINGS AND INTERNAL VALUES

(deftemplate MAIN::subject "These are subject's peculiar 
							characteristics detected by F.A.C.E."
   (slot id (type INTEGER) (default ?DERIVE))
   (slot idKinect (type INTEGER) (default ?DERIVE))
   (slot trackedState (type SYMBOL) (default False) (allowed-symbols False True))
   (multislot name (type LEXEME) (default unknown))
   (slot gender (type SYMBOL) (default unknown) (allowed-symbols Male Female unknown))
   (slot age (type INTEGER) (default ?DERIVE))
   (slot speak_prob)
   (slot gesture (type INTEGER) (default 0))
   (slot uptime (type NUMBER) (default ?DERIVE) (range 0.0 ?VARIABLE))
   (slot angle)
   
   (slot happiness_ratio (type NUMBER) (default ?DERIVE))
   (slot anger_ratio (type NUMBER) (default ?DERIVE))
   (slot sadness_ratio (type NUMBER) (default ?DERIVE))
   (slot surprise_ratio (type NUMBER) (default ?DERIVE))
   
   (multislot head (type NUMBER) (default ?DERIVE))
   (multislot neck (type NUMBER) (default ?DERIVE))
   (multislot spineShoulder (type NUMBER) (default ?DERIVE))
   (multislot spineBase (type NUMBER) (default ?DERIVE))
   (multislot spineMid (type NUMBER) (default ?DERIVE))
   (multislot shoulderLeft (type NUMBER) (default ?DERIVE))
   (multislot shoulderRight (type NUMBER) (default ?DERIVE))
   (multislot elbowLeft (type NUMBER) (default ?DERIVE))
   (multislot elbowRight (type NUMBER) (default ?DERIVE))
   (multislot wristLeft (type NUMBER) (default ?DERIVE))
   (multislot wristRight (type NUMBER) (default ?DERIVE))
   (multislot handLeft (type NUMBER) (default ?DERIVE))
   (multislot handRight (type NUMBER) (default ?DERIVE))
   (multislot hipLeft (type NUMBER) (default ?DERIVE))
   (multislot hipRight (type NUMBER) (default ?DERIVE))
   (multislot kneeLeft (type NUMBER) (default ?DERIVE))
   (multislot kneeRight (type NUMBER) (default ?DERIVE))
   (multislot ankleLeft (type NUMBER) (default ?DERIVE))
   (multislot ankleRight (type NUMBER) (default ?DERIVE))
   (multislot footLeft (type NUMBER) (default ?DERIVE))
   (multislot footRight (type NUMBER) (default ?DERIVE))
   (multislot handTipLeft (type NUMBER) (default ?DERIVE))
   (multislot handTipRight (type NUMBER) (default ?DERIVE))
   (multislot thumbLeft (type NUMBER) (default ?DERIVE))
   (multislot thumbRight (type NUMBER) (default ?DERIVE))

   (slot LeftEyeClosed (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot RightEyeClosed (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot LookingAway (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot WearingGlasses (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthOpen (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthMoved (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot Engaged (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
)

   
(deftemplate MAIN::surroundings "These are surrounding's peculiar 
								 characteristics detected by F.A.C.E."
   (slot soundAngle (type NUMBER) (default ?DERIVE))
   (slot soundEstimatedX (type NUMBER) (default ?DERIVE))
   (slot soundAverageNorm (type NUMBER) (default ?DERIVE))
   (slot soundDecibelNorm (type NUMBER) (default ?DERIVE))
   (multislot recognizedWord (type LEXEME) (default unknown))
   (slot numberSubject (type INTEGER))
   (multislot ambience (type LEXEME))
   (multislot resolution (type NUMBER))
   (multislot saliency (type NUMBER)) ; first two values are x,y in pixels and third one is weight
   (slot toiMic (type NUMBER))
   (slot toiTemp (type NUMBER))
   (slot toiIR (type NUMBER))
   (slot toiTouch (type LEXEME) (default False) (allowed-symbols False True))
   (slot toiLux (type NUMBER))
)

   
(deftemplate MAIN::winner "This is the winner template; inside you can find ID, point to look and lookrule fired"
   (slot id (type INTEGER) (default 0))
   (multislot point (type NUMBER) (default 0.5 0.5))
   (slot lookrule (type LEXEME) (default none))
)


(deftemplate MAIN::face "This is the robot emotional state template, containing its current mood and expression"
   (multislot mood (type NUMBER) (default 0.0 0.0))
   (multislot ecs (type NUMBER) (default 0.0 0.0))
)


;--------------------------------------------------------INIZIALIZATION

(deffacts MAIN::initialization "Just to initialize some useful facts" 
   (winner)
   (winner_not_chosen)
   (face)
   (tracking_is OFF)
)

;--------------------------------------------------------TRACKING ON OR OFF (POINT OR SUBJECT)
   
(defrule MAIN::check_presence "If, at least, one subject is tracked then tracking is on" 
   (surroundings (numberSubject ?numb))
   ?trackOFF <- (tracking_is OFF)
   (test (> ?numb 0))
=>
   (retract ?trackOFF)
   (assert (tracking_is ON))
)


(defrule MAIN::check_loneliness "If no subject is tracked then tracking is off" 
   (surroundings (numberSubject ?numb))
   (test (eq ?numb 0))
=>
   (assert (tracking_is OFF))
)
 
  
(defrule MAIN::refresh_loneliness "If you were lonely at the beginning you will be it also when nobody is inside!"
   ?trackON <- (tracking_is ON)
   (tracking_is OFF)
=>
   (retract ?trackON)
)

   
(defrule MAIN::boring_loneliness "If the robot doesn't see anyone it's bored and follows the virtual point"
   ?check <- (winner_not_chosen)
   (tracking_is OFF)
   ?surround <- (surroundings (saliency ?x ?y ?) (resolution ?w ?h))
   ?win <- (winner) 
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?nx (/ ?x ?w))
   (bind ?ny (- 1 (/ ?y ?h)))
   (modify ?face (ecs -0.30 -0.45))
   (modify ?win (id 1) (point ?nx ?ny) (lookrule LONELINESS))
   (retract ?surround ?check)
   (assert (winner_is_chosen))
)


(defrule MAIN::presence_of_a_subject "This rule selects the winner as the person who is in front of the robot"
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face)
   =>
   (modify ?face (ecs 0.52 0.67))
   (modify ?win (id ?id) (point ?x ?y) (lookrule LOOK_AT_YOU))
   (retract ?check) 
   (assert (winner_is_chosen))
)

;--------------------------------------------------------EXECUTIVE RULE [ "?id" | "?x" | "?y" ] ECS ( "?ev" | "?ea" ) 

(defrule MAIN::look_at_winner "Finally, this rule makes the robot look at the winner making the expression that is the most suitable to the current social context"
   ?check <- (winner_is_chosen)
   ?face <- (face (ecs ?ev ?ea))
   ?win <- (winner (id ?id) (point ?x ?y) (lookrule ?rulefired))
   =>
   (printout t "WINNER [ "?id" ] POINT [ "?x" | "?y" ] ECS [ "?ev" | "?ea" ] RULE [ "?rulefired" ]" crlf)
   ;(fun_lookat ?id ?x ?y)
   ;(fun_makeexp "?ev" "?ea")
   (modify ?win (id 0) (point 0.0 0.0) (lookrule none))
   (modify ?face (ecs 0.0 0.0))
   (retract ?check)
   (assert (CIAO CIAO))
   (assert (delete subjects))
   (assert (delete point))
)


(defrule MAIN::delete_subjects "At the end CLIPS provides to remove all the stored subjects"
   (delete subjects)
   ?sub <- (subject)
   =>
   (retract ?sub)
)

(defrule MAIN::deleting_subjects_is_done "Subjects removal is done!"
   ?del <- (delete subjects)
   (not (subject (idKinect ?)))
   =>
   (retract ?del)
   (assert (winner_not_chosen))
)

(defrule MAIN::delete_point "At the end CLIPS provides to remove the stored point"
   (delete point)
   ?surr <- (surroundings)
   =>
   (retract ?surr)
)

(defrule MAIN::deleting_point_is_done "Point removal is done!"
   ?del <- (delete point)
   (not (surroundings (numberSubject ?)))
   =>
   (retract ?del)
   (assert (winner_not_chosen))
)

