<html>

<h2>how to implement QuestionData.cs </h2>
-> create this script in Scripts/Data/QuestionData.cs <br>

-> create Questions Folder then create Question Asset <br>
&nbsp&nbsp&nbsp -> create / quiz / question, name: "Level01_Question" <br>
&nbsp&nbsp&nbsp -> question: "Was ist die Ursache dafür, dass die Kisten beim Bremsen vom Dach rutschen?" <br>

-> add 4 answers:
<li> 0: "die Motorkraft des Autos" </li>
<li> 1: "die Reibungskraft auf die Kiste" </li>
<li> 2: "die Trägheit der Kisse" ← CORRECT! </li>
<li> 3: "die Trägheit des Autos" </li>

<h2> Add Scripts to GameObjects </h2>
-> Create GameObject "GameManager": <br>
-> Add Component → StateManager <br>
-> Add Component → AnswerSystem <br>

<br>
<i> Assign StateManager fields: </i> 
<br> <br>
-> Title Panel: drag TitlePanel <br>
-> Subtitle Panel: drag SubtitlePanel <br>
-> Quiz Panel: drag QuizPanel <br>
-> Question Panel: drag QuestionPanel <br>
-> Answer Options Panel: drag AnswerOptionsPanel <br>
-> Submit Button: drag SubmitButton <br>
-> Current Question: drag your Level01_Question asset! <br>
-> Question Text: drag QuestionText (TMP) <br>
-> Answer System: drag the GameManager itself (it has AnswerSystem on it) <br>
-> Subtitle Script: drag TimelineManager <br>
-> Timeline: drag the PlayableDirector component <br>

<br>
<i> Assign AnswerSystem fields: </i> 
<br> <br>
-> Answer Options Panel: drag AnswerOptionsPanel <br>
-> State Manager: drag GameManager (it has StateManager on it) <br>
-> Correct Index: DON'T SET THIS! StateManager sets it automatically <br>
-> Set your colors (or leave defaults) <br>

<h2> submit button </h2>
-> Select SubmitButton <br>
-> In Button component, find "On Click ()" <br>
-> Click + <br>
-> Drag GameManager into object field <br>
-> Dropdown: AnswerSystem → CheckAnswer() <br>

</html>

# scene hierarchy

- **Scene**
    - **TimelineManager**
        - PlayableDirector
        - SubtitleScript
    - **Canvas**
        - TitlePanel
        - SubtitlePanel
        - **QuizPanel**
            - **QuestionPanel**
                - `QuestionText` _(TextMeshPro)_
            - **AnswerOptionsPanel**
                - `AnswerOption1` _(Button with Text child)_
                - `AnswerOption2`
                - `AnswerOption3`
                - `AnswerOption4`
            - `SubmitButton`
    - `GameManager` _(empty GameObject)_
