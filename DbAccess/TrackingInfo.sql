SET ANSI_NULLS ON
GO

DROP TABLE [dbo].[TrackingInfo]
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TrackingInfo](
	[TrackingNumber] [CHAR](35) NOT NULL,
	[UserId] [VARCHAR](50) NOT NULL,
	[Note] [char](80) NOT NULL,
	[CityState] [VARCHAR](50) NOT NULL,
	[DeliveryZip] [CHAR](5) NOT NULL,
	[Inbound] [BIT] NOT NULL,
	[Delivered] [BIT] NOT NULL,
	[DeliveryStatus] [VARCHAR](100) NOT NULL,
	[LastEventDatetime] DATETIME NOT NULL
PRIMARY KEY CLUSTERED 
(
	[TrackingNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TrackingInfo]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[UserInfo] ([UserId])
GO

SELECT * FROM [dbo].[TrackingInfo] 
GO

/****** Object:  StoredProcedure [dbo].[GetUndelivered]    Script Date: 9/27/2021 6:33:56 AM ******/
DROP PROCEDURE [dbo].[SaveHistory]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON

GO
CREATE PROCEDURE [dbo].[SaveHistory] (
	@tracking_number VARCHAR(35),
	@user_id VARCHAR(50),
	@note VARCHAR(50),
	@city_state VARCHAR(100),
	@delivery_zip VARCHAR(5),
	@inbound BIT,
	@delivered BIT,
	@delivery_status VARCHAR(MAX),
	@last_event_datetime DATETIME
	)
AS
BEGIN
	IF EXISTS (SELECT * FROM TrackingInfo t WHERE t.TrackingNumber = @tracking_number)
		UPDATE TrackingInfo SET Delivered = @delivered, DeliveryStatus = @delivery_status, LastEventDatetime = @last_event_datetime WHERE TrackingNumber = @tracking_number
	ELSE
		INSERT INTO TrackingInfo (TrackingNumber, UserId, Note, CityState, DeliveryZip, Inbound, Delivered, DeliveryStatus, LastEventDatetime)
		Values (@tracking_number, @user_id, @note, @city_state, @delivery_zip, @inbound, @delivered, @delivery_status, @last_event_datetime)
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON

GO
CREATE PROCEDURE [dbo].[GetSavedHistories] (
	@user_id VARCHAR(50),
	@start_datetime DATETIME
	)
AS
BEGIN
	SELECT * FROM TrackingInfo WHERE Delivered = @delivered AND LastEventDatetime > @start_datetime;
END
GO

