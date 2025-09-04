CREATE TABLE "UsersTeam"
(
	"teamId" uuid PRIMARY KEY,
	"name" varchar(100) NOT NULL
);
CREATE INDEX idx_teams_id ON public."UsersTeam" ("teamId");

CREATE TABLE "ProjectUsers"
(
	"userId" uuid PRIMARY KEY,
	"telegramUserId" bigint NOT NULL,
	"userName" varchar(100) NOT NULL,
	"teamId" uuid NULL,
	"role" int NOT NULL DEFAULT 0,
	"isAdmin" bool DEFAULT FALSE,

	CONSTRAINT "User_TeamId"
    FOREIGN KEY ("teamId") 
    REFERENCES "UsersTeam"("teamId")
);
CREATE INDEX idx_users_id ON public."ProjectUsers" ("userId");
CREATE INDEX idx_telegram_users_id ON public."ProjectUsers" ("telegramUserId");

CREATE TABLE "Projects"
(
	"projectId" uuid PRIMARY KEY,
	"name" varchar(100) NOT NULL,
	"userId" uuid NOT NULL,
	"description" varchar(100) NOT NULL,
	"createdAt" date NOT NULL,
	"deadline" date NOT NULL,
	"status" int NOT NULL,

	CONSTRAINT "Project_UserId"
    FOREIGN KEY ("userId") 
    REFERENCES "ProjectUsers"("userId")
);
CREATE INDEX idx_project_id ON public."Projects" ("projectId");

CREATE TABLE "TaskStages"
(
	"stageId" uuid PRIMARY KEY,
	"name" varchar(100) NOT NULL,
	"comment" varchar(100) NULL,
	"description" varchar(100) NULL,
	"status" int NOT NULL,
	"userId" uuid NOT NULL,
	"nextStageId" uuid NULL,

	CONSTRAINT "Stage_UserId"
    FOREIGN KEY ("userId") 
    REFERENCES "ProjectUsers"("userId")	
);
CREATE INDEX idx_stages_id ON public."TaskStages" ("stageId");


CREATE TABLE "ProjectTasks"
(
	"taskId" uuid PRIMARY KEY,
	"taskName" varchar(100) NOT NULL,
	"teamId" uuid NOT NULL,
	"startStageId" uuid NOT NULL,
	"projectId" uuid NULL,
	"description" varchar(100) NOT NULL,
	"createdAt" date NOT NULL,
	"deadline" date NOT NULL,
	"status" int NOT NULL,

	CONSTRAINT "Task_TeamId"
    FOREIGN KEY ("teamId") 
    REFERENCES "UsersTeam"("teamId"),
	
	CONSTRAINT "Task_StageId"
    FOREIGN KEY ("startStageId") 
    REFERENCES "TaskStages"("stageId"),

	CONSTRAINT "Task_ProjectId"
    FOREIGN KEY ("projectId") 
    REFERENCES "Projects"("projectId")	
);
CREATE INDEX idx_tasks_id ON public."ProjectTasks" ("taskId");