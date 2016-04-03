using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using DM2ContentIndexingDataContract;
using DM2WebDB.CvEvents;
using com.commvault.biz.CvEvent;
using commvault.utils;
using ErrorMessageFramework.ErrorCode;
using ManagedLogging;
using com.commvault.biz.search;
using System.DirectoryServices;
using DM2WebLib.Security;
using CvRest;
using CVWebApi;
using CvSMTP;
using CVManaged;

namespace CVWebService
{
    public partial interface ICVWebSvc
    {
        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/CreateEvent")]

        Message CreateEvent(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/EditEvent")]

        Message EditEvent(Message eventMsg);

        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/EditEventCheck?eventid={eventid}")]

        Message EditEventCheck(int eventid);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/RespondEvent")]

        Message RespondEvent(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/DeleteEvent")]

        Message DeleteEvent(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/GetTimelineEvents")]

        Message GetTimelineEvents(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/GetEventQuestions")]

        Message GetEventQuestions(Message eventMsg);
        
        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/GetUserSuggestionsCvEvents?userPrefix={userPrefix}&loginName={loginName}&pageSize={pageSize}")]

        Message GetUserSuggestionsCvEvents(string userPrefix, string loginName, int pageSize);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/setImageForEvent?eventId={eventId}&filename={filename}")]

        Message SetImageFileNameForEvent(int eventId, string fileName);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/InviteMoreParticipants")]

        Message InviteMoreParticipants(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/getEventResponse")]

        Message getEventResponse(Message eventMsg);
        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/getNotRespondParticipant")]

        Message getNotRespondParticipant(Message eventMsg);

        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/getMailServerDetails")]

        Message getMailServerDetails();

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/resetOldEventDetails?eventId={eventId}")]

        Message resetOldEventDetails(int eventId);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/mailContentOperation")]

        Message mailContentOperation(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/sendSelectionMail")]

        Message sendSelectionMail(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/sendInvitation")]

        Message sendInvitation(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/sendCancelMail")]

        Message sendCancelMail(Message eventMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/sendMail")]

        Message sendMail(Message mailDetailMsg);
		
        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/getInterestedParticipant")]

        Message getInterestedParticipant(Message eventMsg);

		/*Group*/

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/CreateGroup")]

        Message CreateGroup(Message groupMsg);

     

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/EditGroup")]

        Message EditGroup(Message groupMsg);
        

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/DeleteGroup")]

        Message DeleteGroup(Message groupMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/GetTimelineGroups")]

        Message GetTimelineGroups(Message groupMsg);

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/AddMembers")]

        Message AddMembers(Message groupMsg);

        /*Group*/

        /*User profile control*/
        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/ControlUserProfile?userGuid={userGuid}&modifierGuid={modifierGuid}&eventId={eventId}&groupId={groupId}&tags={tags}&isFavorite={isFavorite}&notificationCode={notificationCode}&status={status}")]

        Message ControlUserProfile(string userGuid,string modifierGuid,int eventId,int groupId,string tags, int isFavorite, int notificationCode, int status);
        /*User Profile Control*/


        /*Notfications*/
        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/GetNotifications")]

        Message GetNotifications(Message userMsg);
        /*Notification*/

        /*Get user infomation (to be called each time user login)*/

        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/GetUserInformation")]

        Message GetUserInformation(Message userInfo);

        /*Get user information*/

        /*Check if group exists in db*/

        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/checkIfGroupExists?groupName={groupName}")]

        Message checkIfGroupExists(string groupName);

        /*Get logical group suggestions */
        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/getGroupSuggestions?groupName={groupName}&pageSize={pageSize}")]

        Message getGroupSuggestions(string groupName, int pageSize);

        /*Get user and group suggestions*/
        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/getUserAndGroupSuggestions?namePattern={userNamePart}&pageSize={pageSize}")]

        Message getUserAndGroupSuggestions(string userNamePart, int pageSize);

        /*Get user and group suggestions*/
        [OperationContract, WebInvoke(Method = "GET", UriTemplate = "/getTagSuggestions?pattern={pattern}&pageSize={pageSize}")]

        Message getTagSuggestions(string pattern, int pageSize);

        /*Get user tags*/
        [OperationContract, WebInvoke(Method = "POST", UriTemplate = "/getUserInterest")]

        Message getUserInterest(Message userInfo);
    }

    public partial class CVWebSvc : ICVWebSvc
    {
        
        public Message CreateEvent(Message eventMsg)
        {
            const string function = "CVWebSvc::CreateEvent";
            Message resp = null;
            CvEventResp eventResp = null;           
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    AddUsersToEventReq(eventReq);                    
                    /*if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
                    {
                        for (int eventCount = 0; eventCount < eventReq.events.Count; eventCount++)
                       {
                            List<String> usermailIdList = new List<String>();
                            CvEvent cvEvent = eventReq.events[eventCount];
                            if (cvEvent.eventOrganizersDisp != null && cvEvent.eventOrganizersDisp.Count > 0)
                            {
                                foreach (CvEventUser cvEventUser in cvEvent.eventOrganizersDisp)
                                {
                                    if(!string.IsNullOrEmpty(cvEventUser.smtpAddress))
                                    usermailIdList.Add(cvEventUser.smtpAddress);
                                }
                            }
                            //creating the event organizers list by going through each email id
                            if (usermailIdList != null && usermailIdList.Count > 0)
                            {
                                eventReq.events[eventCount].eventOrganizers = checkIfGroupAndExpand(usermailIdList.ToArray());
                            }
                            usermailIdList.Clear();
                            //creating the event participants list by going through each email id
                            if (cvEvent.eventParticipantsDisp != null && cvEvent.eventParticipantsDisp.Count > 0)
                            {
                                foreach (CvEventUser cvEventUser in cvEvent.eventParticipantsDisp)
                                {
                                    if (!string.IsNullOrEmpty(cvEventUser.smtpAddress))
                                    usermailIdList.Add(cvEventUser.smtpAddress);
                                }
                            }
                            if (usermailIdList != null && usermailIdList.Count > 0)
                            {
                                eventReq.events[eventCount].eventParticipants = checkIfGroupAndExpand(usermailIdList.ToArray());
                            }
                        }

                    }*/
                    eventResp = new dmCvEvent().SaveEvent(eventReq);
                    
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message EditEvent(Message eventMsg)
        {
            const string function = "CVWebSvc::EditEvent";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = this.MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    AddUsersToEventReq(eventReq);
                    eventResp = new dmCvEvent().EditEvent(eventReq);                    
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message DeleteEvent(Message eventMsg)
        {
            const string function = "CVWebSvc::DeleteEvent";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = this.MessageToObject<CvEventReq>(function, eventMsg, false);
            try
            {
                if (eventReq != null)
                {                    
                    eventResp = new dmCvEvent().DeleteEvent(eventReq);                    
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = this.CreateXMlTextMessage(function, eventResp, false);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message GetTimelineEvents(Message eventMsg)
        {
            const string function= "CVWebSvc::GetTimelineEvents";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);            
            try
            {
                if (eventReq != null)
                {
                    CvEventMgmt eventMgmt = new CvEventMgmt();
                    if (eventReq.searchFilter != null && eventReq.searchFilter.filter != null && !string.IsNullOrEmpty(eventReq.searchFilter.filter.filterName))
                    {
                        if (eventReq.searchFilter.filter.filterName.Equals(CvEventFilterType.TimeFilter.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            List<long> time = eventMgmt.ExtractTimeFilterValues(eventReq);
                            eventResp = new dmCvEvent().SearchEvents(eventReq, time[0], time[1],false);
                            if (eventResp != null && eventResp.events != null)
                            {
                                eventResp = GetReqInfoFromEventResp(eventResp, eventReq.userInformation.userGuid, eventReq.eventSearchType);
                                
                            }
                        }
                    }
                    else
                    {
                        eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0, false);
                    }
                    
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                        eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message getEventResponse(Message eventMsg)
        {
            const string function = "CVWebSvc::getEventResponse";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0, true,true);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                        eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }        

        public Message getNotRespondParticipant(Message eventMsg)
        {
            const string function= "CVWebSvc::getNotRespondParticipant";
            Message resp = null;
            CvEventResp eventResp=null;
            CvEventUsers userList = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0,false);
                    if (eventResp != null && eventResp.events != null)
                    {
                        CvEvent cvEvent=eventResp.events[0];
                        if (cvEvent.eventParticipants != null)
                        {
                            userList = new CvEventUsers();
                            userList.users = cvEvent.eventParticipants;
                        } 
                    }                    

                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                        eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (userList == null)
                    {
                        userList = new CvEventUsers();
                    }
                    resp = CreateXMlTextMessage(function, userList);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message getMailServerDetails()
        {
            const string function = "CVWebSvc::getMailServerDetails";
            Message resp = null;
            MailServerDetails mailServerDetails = null;
            try
            {
                mailServerDetails = CvEventsHelper.getMailServerDetails();
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {                
                resp = CreateXMlTextMessage(function, mailServerDetails);
            }
            return resp;
        }
        public Message GetEventQuestions(Message eventMsg)
        {
            const string function = "CVWebSvc::GetEventQuestions";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
             try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0,true);
                }                 
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                        eventResp, function, true);
                }                 
             }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        private CvEventResp GetReqInfoFromEventResp(CvEventResp eventResp,String userGUID,CvEventSearchType searchType)
        {
            const string function = "CVWebSvc::GetReqInfoFromEventResp";
            CvEventResp eventRespToSend = null;          
            try
            {                
                if (eventResp != null&& eventResp.events!=null&& eventResp.events.Count>0)
                {
                    eventRespToSend = new CvEventResp();
                    eventRespToSend.events= new List<CvEvent>();
                    eventRespToSend.totalEvents = eventResp.totalEvents;    //storing total events
                        foreach(CvEvent cvevent in eventResp.events)
                        {   
                            Boolean isOrganizer=false;
                            Boolean isParticipant=false;
                            cvevent.eventParticipantsDisp=null;
                            if (searchType != CvEventSearchType.UpcomingEventSearchDetail&&((cvevent.eventType&Convert.ToInt32(CvEventType.EVENT))==Convert.ToInt32(CvEventType.EVENT)))
                                cvevent.eventQuestions=null;
                            if (cvevent.eventOrganizers != null)
                            {
                                foreach (CvEventUser user in cvevent.eventOrganizers)
                                {
                                    if (String.Equals(user.userGuid, userGUID, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        cvevent.eventOrganizers = new List<CvEventUser>();
                                        cvevent.eventOrganizers.Add(user);
                                        isOrganizer = true;
                                    }
                                }
                            }
                            if(!isOrganizer)
                            {
                                cvevent.eventOrganizers=null;
                            }
                            if (cvevent.eventParticipants != null)
                            {
                                foreach (CvEventUser user in cvevent.eventParticipants)
                                {
                                    if (String.Equals(user.userGuid, userGUID, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        cvevent.eventParticipants = new List<CvEventUser>();
                                        cvevent.eventParticipants.Add(user);
                                        isParticipant = true;
                                    }
                                }
                            }
                            if(!isParticipant)
                            {
                                cvevent.eventParticipants=null;
                            }
                            eventRespToSend.events.Add(cvevent);                            
                        }

                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_RESPONSE, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_RESPONSE_EMPTY,
                        eventRespToSend, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventRespToSend);
            }
            return eventRespToSend;
        }


        public Message EditEventCheck(int eventid)
        {
            const string func = "CvEventsMessages::EditEventCheck";
            Message result = null;
            CvEntitiesMsg.UserEntity user = null;
            CVWebApi.WebSessionInfo.GetLoggedInUserInfo(out user);
            bool safeToEditEvent = new dmCvEvent().EditEventCheck(eventid, user.userGUID);
            result = CreatePlainTextMessage(func, safeToEditEvent.ToString());
            return result;
        }

        public Message RespondEvent(Message eventMsg)
        {
            const string function = "CVWebSvc::RespondEvent";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {                   
                    eventResp = new dmCvEvent().RespondEvent(eventReq);                   
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message GetUserSuggestionsCvEvents(string userPrefix, string loginName, int pageSize)
        {
            Message resp = null;
            //CvEventUsers suggestions = new CvEventUsers();
            //suggestions.users = new List<CvEventUser>();

            //CvEventReq dummyEventReq = new CvEventReq();
            //dummyEventReq.userInformation = new UserInfo();
            //loginName = loginName.Trim();
            //loginName = loginName.Trim(new char[]{'\"'});
            //DomainUser domainUserCombination = SearchUtils.GetDomainUser(loginName.Trim());

            //if (null != domainUserCombination &&
            //    string.IsNullOrEmpty(domainUserCombination.DomainName) == false &&
            //    string.IsNullOrEmpty(domainUserCombination.UserName) == false)
            //{
            //    dummyEventReq.userInformation = new UserInfo();
            //    dummyEventReq.userInformation.aliasName = domainUserCombination.UserName;
            //    dummyEventReq.userInformation.domain = new DomainInfo();
            //    dummyEventReq.userInformation.domain.domainName = domainUserCombination.DomainName;
            //    CvEventMgmt dummyEventMgmt = new CvEventMgmt();
            //    GenericResp domainPopulation = dummyEventMgmt.PopulateDomainInfo(dummyEventReq);
            //    if (domainPopulation != null && domainPopulation.errList != null && domainPopulation.errList.Count > 0)
            //    {
            //        if (domainPopulation.errList[0].errorCode == 0)
            //        {
            //            DomainInfo domainInformation = dummyEventReq.userInformation.domain;
            //            string adPath = "LDAP://" + domainInformation.hostName;
            //            DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(adPath, domainInformation.serviceUserName,
            //                DM2WebLib.dmConf.GetDecryptedPassword(domainInformation.servicePassword));
            //            DirectorySearcher searcher = new DirectorySearcher(entry);
            //            searcher.Filter = (string.Format(" (&(|(objectClass=user)(objectClass=group))(!userAccountControl:1.2.840.113556.1.4.803:=2)(|(mail={0}*)(displayName={0}*)(samaccountname={0}*)))", userPrefix));
            //            searcher.PropertiesToLoad.AddRange(new string[] { "mail", "displayName", "objectGuid", "samaccountname" });
            //            searcher.SizeLimit = pageSize > 0 ? pageSize : 20;
            //            SortOption option = new SortOption("Mail", System.DirectoryServices.SortDirection.Ascending);
            //            searcher.Sort = option;
            //            SearchResultCollection results = searcher.FindAll();
            //            string displayName, smtpAddr, objectGuid, aliasName;
            //            foreach (SearchResult resEnt in results)
            //            {
            //                displayName = string.Empty;
            //                smtpAddr = string.Empty;
            //                objectGuid = string.Empty;
            //                aliasName = string.Empty;
            //                DirectoryEntry de = resEnt.GetDirectoryEntry();
            //                if (de.Properties["displayName"] != null && de.Properties["displayName"].Value != null)
            //                {
            //                    displayName = de.Properties["displayName"].Value.ToString();
            //                }
            //                if (de.Properties["mail"] != null && de.Properties["mail"].Value != null)
            //                {
            //                    smtpAddr = de.Properties["mail"].Value.ToString();
            //                }
            //                if (de.Properties["samaccountname"] != null && de.Properties["samaccountname"].Value != null)
            //                {
            //                    aliasName = de.Properties["samaccountname"].Value.ToString();
            //                }
            //                if (de.Properties["objectGuid"] != null && de.Properties["objectGuid"].Value != null)
            //                {
            //                    byte[] bObjectGuid = (byte[])de.Properties["objectGuid"].Value;
            //                    if ((bObjectGuid == null) || (bObjectGuid.Length == 0))
            //                    {
            //                        objectGuid = Guid.Empty.ToString();
            //                    }
            //                    else
            //                    {
            //                        objectGuid = new Guid(bObjectGuid).ToString();
            //                    }
            //                }
            //                if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(smtpAddr) && !string.IsNullOrEmpty(objectGuid) && !string.IsNullOrEmpty(aliasName))
            //                {
            //                    CvEventUser user = new CvEventUser();
            //                    user.displayName = displayName;
            //                    user.smtpAddress = smtpAddr;
            //                    user.userGuid = objectGuid;
            //                    user.aliasName = aliasName;
            //                    suggestions.users.Add(user);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            // Log error and return 0 suggestions
            //            //
            //        }
            //    }
            //}
            //else
            //{
            //    // Log error and return 0 suggestions
            //    //
            //}
            //try
            //{
            //    resp = this.CreateXMlTextMessage(CVWebSvc.function, suggestions, false);
            //}
            //catch (Exception ex)
            //{
            //    this.logger.logException(function, ex);
            //}            
      


            return resp;
        }

        public Message SetImageFileNameForEvent(int eventId, string fileName)
        {
            const string function = "CVWebSvc::SetImageFileNameForEvent";
            Message respMsg = null;
            GenericResp resp = null;
            try
            {
                resp = new dmCvEvent().updateImageFileInEvent(eventId, fileName);                
            }
            catch (Exception ex)
            {
                logger.logException(function, ex);
                resp = new GenericResp();
                resp.errList = new List<Error>();
                Error err = new Error();
                err.errLogMessage = CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED;
                err.errorCode = (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED;
                resp.errList.Add(err);
            }
            finally
            {
                try
                {
                    if (resp == null)
                    {
                        resp = new GenericResp();
                    }
                    respMsg = CreateXMlTextMessage(function, resp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }
        public Message resetOldEventDetails(int eventId)
        {
            const string function = "CVWebSvc::resetOldEventDetails";
            Message respMsg = null;
            GenericResp resp = null;
            try
            {
                resp = new dmCvEvent().resetOldEventDetails(eventId);                
            }
            catch (Exception ex)
            {
                logger.logException(function, ex);
                resp = new GenericResp();
                resp.errList = new List<Error>();
                Error err = new Error();
                err.errLogMessage = CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED;
                err.errorCode = (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED;
                resp.errList.Add(err);
            }
            finally
            {
                try
                {
                    if (resp == null)
                    {
                        resp = new GenericResp();
                    }
                    respMsg = CreateXMlTextMessage(function, resp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }
        

        public Message InviteMoreParticipants(Message eventMsg)
        {
            const string function ="CVWebSvc::InviteMoreParticipants";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            List<CvEventUser> newEventParticipant = null;
            List<CvEventUser> newEventParticipantResp = null; 
            try
            {
                if (eventReq != null)
                {
                    if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
                    {
                        //expand the new user list
                        if (eventReq.events[0].eventParticipantsDisp != null)
                        {
                            List<String> usermailIdList = new List<String>();
                            CvEvent cvEvent = eventReq.events[0];

                            if (cvEvent.eventParticipantsDisp != null && cvEvent.eventParticipantsDisp.Count > 0)
                            {
                                foreach (CvEventUser cvEventUser in cvEvent.eventParticipantsDisp)
                                {
                                    usermailIdList.Add(cvEventUser.smtpAddress);
                                    
                                }
                            }
                            if (usermailIdList != null && usermailIdList.Count > 0)
                            {
                                newEventParticipant = checkIfGroupAndExpand(usermailIdList.ToArray());
                            }

                            for (int i = 0; i < newEventParticipant.Count; i++)
                            {
                                newEventParticipant[i].groupType = GroupType.NONE;
                                newEventParticipant[i].notificationCode = NotificationPref.ON;
                            }

                            List<CvEventUser> adGroups = new List<CvEventUser>();
                            adGroups = checkAndGetADgroups(usermailIdList.ToArray());
                            if (adGroups != null && adGroups.Count > 0)
                            {
                                for (int i = 0; i < adGroups.Count; i++)
                                {
                                    adGroups[i].userType = CvEventUserType.MEMBER;
                                }
                                newEventParticipant.AddRange(adGroups);
                            }

                        }
                        if (newEventParticipant == null || eventReq.events[0].eventParticipantsDisp == null)
                        {
                            AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_PARTICIPANT_LIST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_PARTICIPANT_NOT_PROVIDED,
                       eventResp, function, true);
                        }
                        else
                        {
                            //fetch the existing participant user list

                            CvEventResp searchEventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0, false);
                            List<CvEventUser> participantDispList = null;
                            newEventParticipantResp = new List<CvEventUser>();
                            Dictionary<string, CvEventUser> distCveventUserList = new Dictionary<string, CvEventUser>(
  StringComparer.OrdinalIgnoreCase);
                            eventReq.events[0].eventParticipants = new List<CvEventUser>();
                            if (searchEventResp != null && searchEventResp.events != null && searchEventResp.events.Count > 0)
                            {
                                CvEvent eventDetail = new CvEvent();
                                eventDetail = searchEventResp.events[0];
                                //updating the participant list
                                if (eventDetail.eventParticipantsDisp != null)
                                {
                                    participantDispList = eventDetail.eventParticipantsDisp;                                    
                                    foreach (CvEventUser user in eventReq.events[0].eventParticipantsDisp)
                                    {                                        
                                        if (!participantDispList.Contains(user))
                                        {
                                            participantDispList.Add(user);
                                        }                                        
                                    }
                                    eventReq.events[0].eventParticipantsDisp = participantDispList;                                    
                                }
                                if (eventDetail.eventParticipants == null)
                                {
                                    eventReq.events[0].eventParticipants = newEventParticipant;
                                    newEventParticipantResp = newEventParticipant;
                                }
                                else
                                {
                                    
                                    foreach (CvEventUser eventUser in eventDetail.eventParticipants)
                                    {
                                        if(!string.IsNullOrWhiteSpace(eventUser.smtpAddress))
                                            distCveventUserList.Add(eventUser.smtpAddress, eventUser);
                                        eventReq.events[0].eventParticipants.Add(eventUser);
                                        //adding existing participant to the event req for invite more
                                    }
                                    foreach (CvEventUser user in newEventParticipant)
                                    {
                                        //adding new participant which are not present in db
                                        if (!distCveventUserList.ContainsKey(user.smtpAddress))
                                        {
                                            //getting only the expanded list of new participants
                                            //to send it back to the client to send mail only to newly added participants.
                                            newEventParticipantResp.Add(user);
                                            eventReq.events[0].eventParticipants.Add(user);
                                        }
                                    }
                                }
                                //updating event with new participants
                                eventResp = new dmCvEvent().SaveEvent(eventReq, true,true);
                                if (eventResp != null && eventResp.events[0]!=null &eventResp.events[0].eventParticipants!=null && eventResp.events[0].eventParticipants.Count>0)
                                {
                                    eventResp.events[0].eventParticipants = newEventParticipantResp;
                                    //sending only the list of new participants 
                                    //As mail will be send only to newly added participants.
                                }
                            }
                            else
                            {
                                AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_RESPONSE, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_NOT_EXIST,
                       eventResp, function, true);
                            }
                        }
                    }
                    else
                    {
                        AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                    }

                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp; 
        }

        private void AddEventResponseError(String msg, int errCode, CvEventResp eventResp, string function = "", bool log = false)
        {
            Error err = new Error();
            err.errLogMessage = msg;
            err.errorCode = errCode;
            if (eventResp == null)
                eventResp = new CvEventResp();
            if (eventResp.errorDetail == null)
                eventResp.errorDetail = new GenericResp();
            if (eventResp.errorDetail.errList == null)
                eventResp.errorDetail.errList = new List<Error>();
            eventResp.errorDetail.errList.Add(err);
            if (log)
            {
                this.logger.DebugLvl(function, LogLevel.LOG_ERROR, msg);
            }
        }

        public class UserList
        {            
            public Dictionary<string, CvEventUser> distCveventUserList
            {
                get;
                set;
            }
        }
        public List<DM2ContentIndexingDataContract.DomainInfo> getDomainInfoList()
        {
            const string function = "CVWebSvc::getDomainInfoList";
            List<DM2ContentIndexingMsg.DomainInfo> domainInfoList = WebUtilities.GetDomainInfoList(WebUtilities.GetProviders(true),
                convertToContractNs: false, ignoreCS: true, ignoreBlackListedDomains: false,
                serviceType: ((int)com.commvault.biz.search.ProviderType.AD));
            
            List<DM2ContentIndexingDataContract.DomainInfo> domainInfoListDCList= new List<DomainInfo>();

            for (int i = 0; i < domainInfoList.Count; i++)
            {
                Object objDC = com.commvault.biz.common.GenericConverter.Convert((object)domainInfoList[i],
                typeof(DM2ContentIndexingMsg.DomainInfo),
                typeof(DM2ContentIndexingDataContract.DomainInfo));


                if (objDC != null && objDC is DM2ContentIndexingDataContract.DomainInfo)
                {
                    DM2ContentIndexingDataContract.DomainInfo domainInfoListDC = objDC as DM2ContentIndexingDataContract.DomainInfo;
                    domainInfoListDCList.Add(domainInfoListDC);
                }
                else
                {
                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "Input Conversion Failed..");
                    return domainInfoListDCList;
                }
            }
            if (domainInfoListDCList == null || domainInfoListDCList.Count == 0)
            {
                logger.DebugLvl(function, LogLevel.LOG_ERROR, "No registered name servers (domains) found for fetching user suggestions from active directory");
            }

            else
            {
                string domainName = QueryString.GetString(DM2WebLib.RestParamName.DOMAIN_NAME);

                if (!string.IsNullOrEmpty(domainName))
                {

                    DM2ContentIndexingMsg.DomainInfo targetDomain = domainInfoList.Find(m => string.Equals(m.domainName, domainName, StringComparison.OrdinalIgnoreCase));
                    if (targetDomain != null)
                    {
                        Object objDC = com.commvault.biz.common.GenericConverter.Convert((object)targetDomain,
                                    typeof(DM2ContentIndexingMsg.DomainInfo),
                                        typeof(DM2ContentIndexingDataContract.DomainInfo));


                        if (objDC != null && objDC is DM2ContentIndexingDataContract.DomainInfo)
                        {
                            DM2ContentIndexingDataContract.DomainInfo domainInfoListDC = objDC as DM2ContentIndexingDataContract.DomainInfo;
                            domainInfoListDCList.Add(domainInfoListDC);
                        }
                        else
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Input Conversion Failed..");
                            return domainInfoListDCList;
                        }
                    }
                    else
                    {
                        logger.DebugLvl(function, LogLevel.LOG_ERROR, "Requested domain [{0}] not found", domainName);
                    }
                }
            }
            return domainInfoListDCList;
        }

        public List<CvEventUser> checkIfGroupAndExpand(String[] mailIds)
        {
            const string function = "CVWebSvc::checkIfGroupAndExpand";
            List<DM2ContentIndexingDataContract.DomainInfo> domainInfoList = getDomainInfoList();
            UserList userList= new UserList();
            userList.distCveventUserList = new Dictionary<string, CvEventUser>(
  StringComparer.OrdinalIgnoreCase);
            List<CvEventUser> cvEventUserList = new List<CvEventUser>();
            Dictionary<string, DomainInfo> domainMapWithMailSuffix = new Dictionary<string, DomainInfo>(
  StringComparer.OrdinalIgnoreCase);
            String[] mailIdList = Array.ConvertAll<String, String>(mailIds, delegate(String s) { return s.ToLower(); });
            List<String> notAdMailIdList = new List<String>();
            List<CvEventUser> cvEventUserListTemp = null;
            try
            {
                foreach (String eachMailId in mailIdList)
                {
                    cvEventUserListTemp = new List<CvEventUser>();
                    if (String.IsNullOrEmpty(eachMailId) || String.IsNullOrEmpty(eachMailId.Trim()))
                        continue;
                    string eachmail = eachMailId.Trim();
                    string aDMapVal = eachmail.Substring(eachmail.IndexOf('@') + 1);
                    //if no domain exists in the name server then simply add the mail id as each field of cveventuser object
                    if (domainInfoList.Count == 0)
                    {
                        logger.DebugLvl(function, LogLevel.LOG_ERROR, "No name server is configured.");
                        CvEventUser cvEventUser = new CvEventUser();
                        cvEventUser.smtpAddress = cvEventUser.userGuid = cvEventUser.displayName = eachMailId;
                        if (!userList.distCveventUserList.ContainsKey(cvEventUser.smtpAddress))
                        {
                            userList.distCveventUserList.Add(cvEventUser.smtpAddress, cvEventUser);
                        }
                       // userList.distCveventUserList.Add(cvEventUser.smtpAddress, cvEventUser);
                    }
                    else
                    {
                        for (int i = 0; i < domainInfoList.Count; i++)
                        {
                            DomainInfo domainInfo = null;
                            if (domainMapWithMailSuffix.ContainsKey(aDMapVal))
                            {
                                domainInfo = domainMapWithMailSuffix[aDMapVal];
                            }
                            if (domainInfo == null)
                            {
                                domainInfo = domainInfoList[i];
                            }
                            //geting the list of users or a single user depending upon the mail id
                            cvEventUserListTemp = getUserListInDomain(eachmail, domainInfo);
                            if ((cvEventUserListTemp != null) && (cvEventUserListTemp.Count != 0))
                            {

                                foreach (CvEventUser user in cvEventUserListTemp)
                                {
                                    if (!userList.distCveventUserList.ContainsKey(user.smtpAddress))
                                    {
                                        userList.distCveventUserList.Add(user.smtpAddress, user);
                                    }
                                }
                                if (!domainMapWithMailSuffix.ContainsKey(aDMapVal))
                                {
                                    domainMapWithMailSuffix.Add(aDMapVal, domainInfoList[i]);
                                }
                                break;
                            }
                        }
                        //not belongs to group or AD user 
                        if ((cvEventUserListTemp == null) || (cvEventUserListTemp.Count == 0))
                        {
                            notAdMailIdList.Add(eachmail);                            
                            //if (!userList.distCveventUserList.ContainsKey(eachmail))
                            //{
                            //    CvEventUser cvEventUser = new CvEventUser();
                            //    cvEventUser.smtpAddress = cvEventUser.userGuid = cvEventUser.displayName = eachMailId;
                            //    userList.distCveventUserList.Add(cvEventUser.smtpAddress, cvEventUser);
                            //    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Mail Id[{0}] does not belong to a group or user.", eachmail);
                            //    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "User details for the mail ID {0} is:", eachmail);
                            //    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "DisplayName {0}:", cvEventUser.displayName);
                            //    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "SMTPAddress {0}:", cvEventUser.smtpAddress);
                            //    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "UserGuid {0}:", cvEventUser.userGuid);
                            //}
                        }
                    }
                }

                //if mailid belongs to commcell user then fetch it from umusers
                if (notAdMailIdList != null && notAdMailIdList.Count > 0)
                {
                    cvEventUserListTemp = new dmCvEvent().GetCvEventsCommcellUsersDetail(notAdMailIdList);
                    if ((cvEventUserListTemp != null) && (cvEventUserListTemp.Count != 0))
                    {
                        foreach (CvEventUser user in cvEventUserListTemp)
                        {
                            if (!userList.distCveventUserList.ContainsKey(user.smtpAddress))
                            {
                                userList.distCveventUserList.Add(user.smtpAddress, user);
                            }
                        }
                    }
                }

                if (userList.distCveventUserList != null && userList.distCveventUserList.Count > 0)
                {
                    foreach (KeyValuePair<string, CvEventUser> cvEventUser in userList.distCveventUserList)
                    {
                        if(!cvEventUserList.Contains(cvEventUser.Value))
                            cvEventUserList.Add(cvEventUser.Value);
                    }
                }
                else
                {
                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "User List is empty");
                }

            }
            catch (Exception ex)
            {
                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
            }
            return cvEventUserList;
        }
        //gives the list of cveventuser if the mail id is group alias other wise returns the individual user details 
        private List<CvEventUser> getUserListInDomain(string emailId, DomainInfo domainInfo)
        {
            const string function = "CVWebSvc::getUserListInDomain";
            string searchPattern = string.Empty;
            string adPath = string.Empty, adHost = string.Empty;
            CvEventUser eventUser= new CvEventUser();
            List<CvEventUser> eventUsersList = new List<CvEventUser>();
            List<String> listOfGroups = new List<String>();
            try
            {
                adHost = domainInfo.hostName;
                adPath = (domainInfo.secureLDAP ? "LDAPS://" : "LDAP://") + domainInfo.hostName;
                String searchFilterForGroup = "(&(objectClass=group)(mail={0}))";
                String searchFilterForUser = "(&(objectClass=user)(mail={0}))";
                String []searchForUser = new String[] { "mail", "displayName", "objectGuid", "samaccountname" };
                String[] searchForGroup = new String[] { "distinguishedName" };

                DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(adPath, domainInfo.serviceUserName,
                    DM2WebLib.dmConf.GetDecryptedPassword(domainInfo.servicePassword));
                DirectorySearcher searcher = new DirectorySearcher(entry);
                searcher.Filter = searchPattern = (string.Format(searchFilterForUser,emailId));
                searcher.PropertiesToLoad.AddRange(searchForUser);
                //searcher.SizeLimit = ;             
                SearchResultCollection results = searcher.FindAll();
                if ((results != null) && (results.Count > 0))
                {
                    eventUsersList = addUserListInCvEventUser(results);

                    /*to store login name(i.e domainname\username) in UMUsers table */
                    for (int i = 0; i < eventUsersList.Count; i++)
                    {
                        eventUsersList[i].loginName = domainInfo.domainName + "\\" + eventUsersList[i].aliasName;
                    }

                    return eventUsersList;
                }
                else
                {
                    //group or not exist in name server
                    searcher.PropertiesToLoad.AddRange(searchForGroup);
                    searcher.Filter = searchPattern = (string.Format(searchFilterForGroup, emailId));
                    results = searcher.FindAll();
                    Boolean isGroup = false;
                    foreach (SearchResult result in results)
                    {
                        String groupName = "";
                        if (result != null)
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Mail Id[{0}] belongs to an group.", emailId);
                            DirectoryEntry de = result.GetDirectoryEntry();
                            if (de.Properties["distinguishedName"] != null && de.Properties["distinguishedName"].Value != null)
                            {
                                String value = de.Properties["distinguishedName"].Value.ToString();
                                groupName = value.Trim();
                            }
                        }
                        listOfGroups.Add(groupName);
                        isGroup = true;
                    }
                    if (!isGroup)
                    {
                        //nemail id not exists in domain name servers                       
                        //CvEventUser cvEventUser = new CvEventUser();
                        //cvEventUser.smtpAddress = cvEventUser.userGuid = cvEventUser.displayName = emailId;
                        //eventUsersList.Add(cvEventUser);
                        return eventUsersList;
                    }
                    else
                    {
                        if (listOfGroups != null && listOfGroups.Count > 0)
                        {
                            string filter = "";
                            foreach (String eachDistingName in listOfGroups)
                            {
                                if (!String.IsNullOrEmpty(eachDistingName))
                                {
                                    filter += "(memberof:1.2.840.113556.1.4.1941:=" + eachDistingName + ")";
                                }
                            }
                            if (!String.IsNullOrEmpty(filter))
                            {
                                String searchFilter = "(&(objectClass=user)(|" + filter + "))";
                                searcher.PropertiesToLoad.AddRange(searchForUser);
                                searcher.Filter = searchFilter;
                                results = searcher.FindAll();
                                if ((results != null) && (results.Count > 0))
                                {
                                    eventUsersList = addUserListInCvEventUser(results);
                                    /*to store login name(i.e domainname\username) in UMUsers table */
                                    for (int i = 0; i < eventUsersList.Count;i++)
                                    {
                                        eventUsersList[i].isDomainGroupMember = true;   //if user is a part of an adgroup
                                        eventUsersList[i].loginName = domainInfo.domainName + "\\" + eventUsersList[i].aliasName;
                                    }
                                    return eventUsersList;
                                }
                            }
                        }                      
                    }
                }               
            }
            catch (Exception ex)
            {            
                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
            }
            finally
            {                
            }
            return eventUsersList;
        }

        

        private List<CvEventUser> addUserListInCvEventUser(SearchResultCollection results) 
        {
            const string function = "CVWebSvc::addUserListInCvEventUser";
            string displayName, smtpAddr, objectGuid, aliasName;
            CvEventUser eventUser = null;
            List<CvEventUser> eventUsersList = new List<CvEventUser>();
            try
            {
                foreach (SearchResult resEnt in results)
                {
                    displayName = string.Empty;
                    smtpAddr = string.Empty;
                    objectGuid = string.Empty;
                    aliasName = string.Empty;
                    DirectoryEntry de = resEnt.GetDirectoryEntry();
                    if (de.Properties["displayName"] != null && de.Properties["displayName"].Value != null)
                    {
                        displayName = de.Properties["displayName"].Value.ToString();
                    }
                    if (de.Properties["mail"] != null && de.Properties["mail"].Value != null)
                    {
                        smtpAddr = de.Properties["mail"].Value.ToString();
                    }
                    if (de.Properties["samaccountname"] != null && de.Properties["samaccountname"].Value != null)
                    {
                        aliasName = de.Properties["samaccountname"].Value.ToString();
                    }
                    if (de.Properties["objectGuid"] != null && de.Properties["objectGuid"].Value != null)
                    {
                        byte[] bObjectGuid = (byte[])de.Properties["objectGuid"].Value;
                        if ((bObjectGuid == null) || (bObjectGuid.Length == 0))
                        {
                            objectGuid = Guid.Empty.ToString();
                        }
                        else
                        {
                            objectGuid = new Guid(bObjectGuid).ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(smtpAddr) && !string.IsNullOrEmpty(objectGuid) && !string.IsNullOrEmpty(aliasName))
                    {
                        eventUser = new CvEventUser();
                        eventUser.displayName = displayName;
                        eventUser.smtpAddress = smtpAddr;
                        eventUser.userGuid = objectGuid;
                        eventUser.aliasName = aliasName;
                        eventUsersList.Add(eventUser);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "user details for the mail ID {0} is:", smtpAddr);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "DisplayName {0}:", eventUser.displayName);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "SMTPAddress {0}:", eventUser.smtpAddress);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "UserGuid {0}:", eventUser.userGuid);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "AliasName {0}:", eventUser.aliasName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
            }
            return eventUsersList;
        }
        public Message mailContentOperation(Message eventMsg)
        {
            const string function = "CVWebSvc::mailContentOperation";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().mailContentOperation(eventReq);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }
        
		public Message sendInvitation(Message eventMsg)
        {
            const string function = "CVWebSvc::sendInvitation";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            bool isSendSuccess = false;
            try
            {
                if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
                {
                    CvEvent cveventReq = eventReq.events[0];
                    List<CvEventMailTypeWithContent> cvEventMailTypeWithContent;
                    String orgMailBody = "";
                    String partMailBody = "";
              
                    if (cveventReq.eventMailContent != null && cveventReq.eventMailContent.cvEventMailContentData != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList.Count > 0)
                    {                        
                        cvEventMailTypeWithContent = cveventReq.eventMailContent.cvEventMailContentData.mailContentList;
                        foreach (CvEventMailTypeWithContent mailTypeWithContent in cvEventMailTypeWithContent)
                        {
                            if (mailTypeWithContent.mailContentType == CvEventMailType.INVITE)                            
                                partMailBody = mailTypeWithContent.mailContent;                       
     
                            else if (mailTypeWithContent.mailContentType == CvEventMailType.ORGANIZER_MAIL)                            
                                orgMailBody = mailTypeWithContent.mailContent;                            
                        }
                    }
                    if ((cveventReq.eventParticipants != null && cveventReq.eventParticipants.Count > 0))
                    {
                        if (!sendMailForEvent(cveventReq, partMailBody, eventReq.userInformation, eventReq.isReminder, false,false,false,false))
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Failed to send mail to some participants");
                            isSendSuccess = false;
                        }
                        else                        
                            isSendSuccess = true;                       
                    }
                    if (cveventReq.eventOrganizers != null && cveventReq.eventOrganizers.Count > 0)
                    {
                        if (!sendMailForEvent(cveventReq, orgMailBody, eventReq.userInformation, eventReq.isReminder, true,false, false,false))
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Failed to send mail to some organizers");
                            isSendSuccess = false;
                        }
                        else
                        {
                           if(isSendSuccess)
                               isSendSuccess = true;
                        }
                            
                    }
                    if (!isSendSuccess)
                    {
                        AddEventResponseError(CvEventExceptionStrings.UNABLE_TO_SEND_MAIL, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL,
                       eventResp, function, true);
                    }
                    else
                    {
                        eventResp = new CvEventResp();
                        eventResp.errorDetail = new GenericResp();
                        eventResp.errorDetail.messageName = "SUCCESS";
                    }
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }
        public Message sendSelectionMail(Message eventMsg)
        {
            const string function = "CVWebSvc::sendSelectionMail";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            bool isSendSuccess = true;
            try
            {
                if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
                {
                    CvEvent cveventReq = eventReq.events[0];
                    List<CvEventMailTypeWithContent> cvEventMailTypeWithContent;
                    String mailBody = "";
                    if (cveventReq.eventMailContent != null && cveventReq.eventMailContent.cvEventMailContentData != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList.Count > 0)
                    {
                        cvEventMailTypeWithContent = cveventReq.eventMailContent.cvEventMailContentData.mailContentList;
                        foreach (CvEventMailTypeWithContent mailTypeWithContent in cvEventMailTypeWithContent)
                        {
                            if (mailTypeWithContent.mailContentType == CvEventMailType.SELECTION_MAIL_NOT_RESP)
                            {
                                mailBody = mailTypeWithContent.mailContent;
                                if (!sendMailForEvent(cveventReq, mailBody, eventReq.userInformation, eventReq.isReminder, false,false,true,false))
                                {
                                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "Failed to send mail to some participants");
                                    isSendSuccess = false;
                                }
                                else
                                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Successfully send mail to participants");
                            }
                            else if (mailTypeWithContent.mailContentType == CvEventMailType.SELECTION_MAIL)
                            {
                                mailBody = mailTypeWithContent.mailContent;
                                if (!sendMailForEvent(cveventReq, mailBody, eventReq.userInformation, eventReq.isReminder, false, false, true, true))
                                {
                                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "Failed to send mail to some participants");
                                    isSendSuccess = false;
                                }
                                else
                                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Successfully send mail to participants");
                            }
                        }
                        
                    }
                    if (!isSendSuccess)
                    {
                        AddEventResponseError(CvEventExceptionStrings.UNABLE_TO_SEND_MAIL, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL,
                       eventResp, function, true);
                    }
                    else
                    {
                        eventResp = new CvEventResp();
                        eventResp.errorDetail = new GenericResp();
                        eventResp.errorDetail.messageName = "SUCCESS";
                    }
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }

            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)                   
                        eventResp = new CvEventResp();                   
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }
        public Message sendCancelMail(Message eventMsg)
        {
            const string function = "CVWebSvc::sendCancelMail";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            bool isSendSuccess = true;
            try
            {
                if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
                {
                    CvEvent cveventReq = eventReq.events[0];
                    List<CvEventMailTypeWithContent> cvEventMailTypeWithContent;
                    String mailBody = "";
                    if (cveventReq.eventMailContent != null && cveventReq.eventMailContent.cvEventMailContentData != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList != null && cveventReq.eventMailContent.cvEventMailContentData.mailContentList.Count > 0)
                    {
                        cvEventMailTypeWithContent = cveventReq.eventMailContent.cvEventMailContentData.mailContentList;
                        foreach (CvEventMailTypeWithContent mailTypeWithContent in cvEventMailTypeWithContent)
                        {
                            if (mailTypeWithContent.mailContentType == CvEventMailType.CANCEL_MAIL)                            
                                mailBody = mailTypeWithContent.mailContent;                           
                        }
                        if (!sendMailForEvent(cveventReq, mailBody, eventReq.userInformation, eventReq.isReminder, false, true, false,false))
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Failed to send mail to some participants");
                            isSendSuccess = false;
                        }
                        else
                            logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Successfully send mail to participants");
                    }                 
                    if (!isSendSuccess)
                    {
                        AddEventResponseError(CvEventExceptionStrings.UNABLE_TO_SEND_MAIL, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL,
                       eventResp, function, true);
                    }
                    else
                    {
                        eventResp = new CvEventResp();
                        eventResp.errorDetail = new GenericResp();
                        eventResp.errorDetail.messageName = "SUCCESS";
                    }
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                       eventResp, function, true);
                }
                
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)                    
                        eventResp = new CvEventResp(); 
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }
        public Message sendMail(Message mailDetailMsg)
        {
            const string function = "CVWebSvc::sendGenericMail";
            Message resp = null;
            GenericResp genResp = new GenericResp();
            genResp.errList = new List<Error>();
            Error err = new Error();
            MailDetails mailReq = null;

            try 
            {
                mailReq = MessageToObject<MailDetails>(function, mailDetailMsg);
                if (mailReq != null )
                    genResp = sendMail(mailReq);
                else
                {
                    err.errorCode = (int)ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY;
                    err.errLogMessage = CvEventExceptionStrings.EMPTY_EVENT_REQUEST;
                    genResp.errList.Add(err);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                err.errorCode = (int)ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL;
                err.errLogMessage = CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED;
                genResp.errList.Add(err);
            }
            finally
            {
                try
                {
                    if (genResp == null)                    
                        genResp = new GenericResp();                    
                    resp = CreateXMlTextMessage(function, genResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public GenericResp sendMail(MailDetails mailDetailReq)
        {
            const string function = "CVWebSvc::sendMail";
            GenericResp genResp = new GenericResp();
            genResp.errList = new List<Error>();
            Error err = new Error();
            string sendTo = "", sendBcc = "", sendCc = "", base64EncodedImage = "", mailBody = "";
            string mailSubject = "", mailFromName = "", mailFrom = "", mailSender = "", mailSenderName = "", imageName = "", errorMsg = "";
            String smtpPassword = null;
            CalendarAttachment attachment1 = null, attachment2 = null;
            MailServerDetails mailServerDetails;
            bool isInlineImage = false;
            int sentMailCounter = 0;
            int totalRecipient = 0;
            try
            {
                if (mailDetailReq != null)
                {
                    mailServerDetails = CvEventsHelper.getMailServerDetails();
                    if (!string.IsNullOrEmpty(mailServerDetails.smtpPassword))
                    {
                        CVPasswordManaged cvPswd = new CVPasswordManaged(false);
                        cvPswd.decrypt(mailServerDetails.smtpPassword, ref smtpPassword);
                    }
                    if (!getMailerDetails(mailDetailReq, ref mailFrom, ref mailFromName, ref mailSender, ref mailSenderName))                    
                        logger.DebugLvl(function, LogLevel.LOG_ERROR, "failed to get mailer details with username and smtpaddress");
                               
                    if (!String.IsNullOrEmpty(mailDetailReq.mailBody))
                        mailBody = mailDetailReq.mailBody;

                    if (!String.IsNullOrEmpty(mailDetailReq.mailSubject))
                        mailSubject = mailDetailReq.mailSubject;

                    if (!String.IsNullOrEmpty(mailDetailReq.base64EncodedImage))
                    {
                        base64EncodedImage = mailDetailReq.base64EncodedImage;
                        isInlineImage = true;
                        imageName = "inlineImage";
                    }
                    if (mailDetailReq.sendAllAtOnce)
                    {
                        if (mailDetailReq.sendTo != null && mailDetailReq.sendTo.Count > 0)
                        {
                            foreach (CvEventUser user in mailDetailReq.sendTo)
                                sendTo += user.smtpAddress + ",";
                            sendTo = CvEventsHelper.removeLastChar(sendTo, ",");
                        }
                        
                        if (mailDetailReq.sendAsBcc != null && mailDetailReq.sendAsBcc.Count > 0)
                        {
                            foreach (CvEventUser user in mailDetailReq.sendAsBcc)
                                sendBcc += user.smtpAddress + ",";
                            sendBcc = CvEventsHelper.removeLastChar(sendBcc, ",");
                        }                        
                        if (mailDetailReq.sendAsCc != null && mailDetailReq.sendAsCc.Count > 0)
                        {
                            foreach (CvEventUser user in mailDetailReq.sendAsCc)
                                sendCc += user.smtpAddress + ",";
                            sendCc = CvEventsHelper.removeLastChar(sendCc, ",");
                        }                        
                        if (!sendMail(mailServerDetails, smtpPassword, sendTo,"", sendBcc, sendCc, mailSubject,
                                mailBody, mailFromName, mailFrom, imageName, base64EncodedImage,
                                isInlineImage, mailSender, mailSenderName, attachment1, attachment2, true, ref errorMsg))
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMail failed  with error [{1}]", errorMsg);
                            err.errorCode = (int)ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL;
                            err.errLogMessage = CvEventExceptionStrings.UNABLE_TO_SEND_MAIL;
                            genResp.errList.Add(err);
                        }
                        else
                        {
                            genResp.messageName = "SUCCESS";
                        }

                    }
                    else
                    {                        
                        if (mailDetailReq.sendTo != null && mailDetailReq.sendTo.Count > 0)
                        {
                            totalRecipient = mailDetailReq.sendTo.Count;
                            foreach (CvEventUser user in mailDetailReq.sendTo)
                            {
                                if (!sendMail(mailServerDetails, smtpPassword, user.smtpAddress,"", sendBcc, sendCc, mailSubject,
                                        mailBody, mailFromName, mailFrom, imageName, base64EncodedImage,
                                        isInlineImage, mailSender, mailSenderName, attachment1, attachment2, true, ref errorMsg))
                                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMail failed for user [{0}] with error [{1}]", user.smtpAddress, errorMsg);                                
                                else
                                    sentMailCounter++;
                            }
                        }
                        if (totalRecipient == sentMailCounter)
                            genResp.messageName = "SUCCESS";
                        else
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMail failed for some participants");
                            err.errorCode = (int)ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL;
                            err.errLogMessage = CvEventExceptionStrings.UNABLE_TO_SEND_MAIL;
                            genResp.errList.Add(err);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                err.errorCode = (int)ErrorCodeConstants.CVEVENTS_ERROR_CODES.FAILED_TO_SEND_MAIL;
                err.errLogMessage = CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED;
                genResp.errList.Add(err);
            }
            finally
            {
                try
                {
                    if (genResp == null)                    
                        genResp = new GenericResp();                    
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return genResp;
        }

        public Boolean sendMailForEvent(CvEvent cveventReq, String mailBody, UserInfo loggedInUser, bool isReminder, bool isForOrganizer, bool isCancelMail,bool isSelectionMail,bool isToRespParticipant)
        {
            const string function = "CVWebSvc::sendMailForEvent";
            CalendarAttachment icsForRSVP = null, icsForResp = null;
            MailServerDetails mailServerDetails;
            String mailFrom ="" ;
            String mailFromName = "";
            String mailSender = "";
            String mailSenderName = "";
            String mailSubject = "";
            String formattedMailBody="";
            string errorMsg = "";
            String smtpPassword = null;
            bool retval = false;
            int sentMailCounter = 0;
            int totalRecipient = 0;
            String organizerSMTP = "";
            try
            {
                mailServerDetails = CvEventsHelper.getMailServerDetails();
                if (loggedInUser != null && !String.IsNullOrEmpty(loggedInUser.smtpAddress))
                    organizerSMTP = loggedInUser.smtpAddress;
                mailSubject = cveventReq.eventMailContent.mailDetails.mailSubject;
                if (!string.IsNullOrEmpty(mailServerDetails.smtpPassword))
                {
                    CVPasswordManaged cvPswd = new CVPasswordManaged(false);
                    cvPswd.decrypt(mailServerDetails.smtpPassword, ref smtpPassword);
                }
                if (!getMailerDetails(cveventReq, ref mailFrom, ref mailFromName, ref mailSender, ref mailSenderName))                
                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "failed to get mailer details with username and smtpaddress");

                if (!isSelectionMail)
                {
                    if (!isReminder)
                    {
                        CalendarAttachment.CARequestStatus requestType = CalendarAttachment.CARequestStatus.Request;
                        if (isCancelMail)
                        {
                            requestType = CalendarAttachment.CARequestStatus.Cancel;
                            logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Request Type is set to CARequestStatus[{0}]", requestType.ToString("G"));
                        }                        
                        icsForRSVP = CvEventsHelper.getAttachment(cveventReq.eventID.ToString()+cveventReq.eventCreationTime.ToString()+"_RSVP", cveventReq.eventAddr.eventAddressLine1,
                                cveventReq.eventMailContent.msgRespond, cveventReq.eventTitle + "(RSVP)", organizerSMTP, cveventReq.eventExpiryDate, 0L, requestType);
                        icsForResp = CvEventsHelper.getAttachment(cveventReq.eventID.ToString() + cveventReq.eventCreationTime.ToString() + "_RESP", cveventReq.eventAddr.eventAddressLine1,
                                cveventReq.eventMailContent.msgRsvp, cveventReq.eventTitle , organizerSMTP, cveventReq.eventScheduledTime, cveventReq.eventScheduledEndTime, requestType);
                    }
                }
                if (isForOrganizer)
                {
                    totalRecipient += cveventReq.eventOrganizers.Count;
                    foreach (CvEventUser user in cveventReq.eventOrganizers)
                    {
                        formattedMailBody = CvEventsHelper.getFormattedMailBody(mailBody, user);
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Formatted email body [{0}]", formattedMailBody);
                        if (!sendMail(mailServerDetails, smtpPassword, user.smtpAddress,user.userGuid, "", "", mailSubject,
                                formattedMailBody, mailFromName, mailFrom, cveventReq.eventImageFilename, cveventReq.eventMailContent.mailDetails.base64EncodedImage,
                                true,mailSender,mailSenderName, icsForResp, icsForRSVP, true, ref errorMsg))                        
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed for user [{0}] with error [{1}]", user.smtpAddress, errorMsg);                        
                        else
                            sentMailCounter++;
                    }
                }
                else if(isCancelMail)
                {                    
                    string bcc = "", cc = "";
                    if (cveventReq.eventOrganizers != null && cveventReq.eventOrganizers.Count > 0)
                    {
                        foreach (CvEventUser user in cveventReq.eventOrganizers)                        
                            cc += user.smtpAddress +",";
                        cc = CvEventsHelper.removeLastChar(cc, ",");
                    }                    
                    if (cveventReq.eventParticipants != null && cveventReq.eventParticipants.Count > 0)
                    {
                        foreach (CvEventUser user in cveventReq.eventParticipants)
                            bcc += user.smtpAddress + ",";
                        bcc = CvEventsHelper.removeLastChar(bcc, ",");
                    }                    
                    if (!sendMail(mailServerDetails, smtpPassword, "","", bcc, cc, mailSubject,
                            mailBody, mailFromName, mailFrom, cveventReq.eventImageFilename, cveventReq.eventMailContent.mailDetails.base64EncodedImage,
                            false, mailSender, mailSenderName, icsForResp, icsForRSVP, true, ref errorMsg))                    
                        logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed  with error [{1}]", errorMsg);                    
                }
                else if(isSelectionMail)
                {
                    if (!isToRespParticipant)
                    {
                        totalRecipient += cveventReq.eventParticipants.Count;
                        foreach (CvEventUser user in cveventReq.eventParticipants)
                        {
                            formattedMailBody = CvEventsHelper.getFormattedMailBody(mailBody, new string[] { user.displayName, user.userGuid });
                            logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Formatted email body [{0}]", formattedMailBody);
                            if (!sendMail(mailServerDetails, smtpPassword, user.smtpAddress,"", "", "", mailSubject,
                                    formattedMailBody, mailFromName, mailFrom, cveventReq.eventImageFilename, cveventReq.eventMailContent.mailDetails.base64EncodedImage,
                                    true, mailSender, mailSenderName, icsForResp, icsForRSVP, true, ref errorMsg))                            
                                logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed for user [{0}] with error [{1}]", user.smtpAddress, errorMsg);                            
                            else
                                sentMailCounter++;
                        }
                    }
                    else
                    {
                        if (!sendSelectionMailWithResp(cveventReq,loggedInUser, mailServerDetails, smtpPassword, mailSubject,mailBody,
                                mailFromName, mailFrom, cveventReq.eventImageFilename, cveventReq.eventMailContent.mailDetails.base64EncodedImage,
                                true, mailSender, mailSenderName, true, ref errorMsg))                        
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed  with error [{1}]", errorMsg);                      
                    }
                }
                else
                {
                    totalRecipient += cveventReq.eventParticipants.Count;
                    foreach (CvEventUser user in cveventReq.eventParticipants)
                    {
                        formattedMailBody = CvEventsHelper.getFormattedMailBody(mailBody, new string[] { user.displayName, user.userGuid });
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Formatted email body [{0}]", formattedMailBody);
                        if (!sendMail(mailServerDetails, smtpPassword,user.smtpAddress,user.userGuid, "", "", mailSubject,
                                formattedMailBody, mailFromName, mailFrom, cveventReq.eventImageFilename, cveventReq.eventMailContent.mailDetails.base64EncodedImage, 
                                true, mailSender, mailSenderName, icsForResp, icsForRSVP, true, ref errorMsg))                        
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed for user [{0}] with error [{1}]", user.smtpAddress, errorMsg);                        
                        else
                            sentMailCounter++;
                    }
                }
                if(sentMailCounter==totalRecipient)
                    retval = true;
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                retval = false;
            }
            return retval;
        }

        public bool getMailerDetails(CvEvent cveventReq, ref string mailFrom, ref string mailFromName, ref string mailSender, ref string mailSenderName)
        {
            const string function = "CVWebSvc::getMailerDetails";
            bool retVal = false;
            try
            {
                if (cveventReq.eventMailContent.mailDetails !=null)
                {
                    retVal= getMailerDetails(cveventReq.eventMailContent.mailDetails, ref mailFrom, ref mailFromName, ref mailSender, ref mailSenderName);
                }
            }
            catch (Exception e)
            {
                this.logger.logException(function, e);
                retVal = false;
            }
            return retVal;
        }

        public bool getMailerDetails(MailDetails mailDetails, ref string mailFrom, ref string mailFromName, ref string mailSender, ref string mailSenderName)
        {
            const string function = "CVWebSvc::getMailerDetails";
            bool retVal = false;
            CvEventUser userInfo = null;
            try
            {

                if (mailDetails != null && mailDetails.mailFrom != null)
                {
                    mailFrom = mailDetails.mailFrom.smtpAddress;
                    mailFromName = mailDetails.mailFrom.displayName;
                    if (String.IsNullOrEmpty(mailFromName))
                    {
                        userInfo = checkIfGroupAndExpand(new string[] { mailFrom })[0];
                        mailFromName = userInfo.displayName;
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Email From Name [{0}] for E-Mail Id[{1}]", mailFromName, mailFrom);
                    }
                }
                if (mailDetails != null && mailDetails.mailSender != null)
                {
                    mailSender = mailDetails.mailSender.smtpAddress;
                    mailSenderName = mailDetails.mailSender.displayName;
                    if (String.IsNullOrEmpty(mailSenderName))
                    {
                        userInfo = checkIfGroupAndExpand(new string[] { mailSender })[0];
                        mailSenderName = userInfo.displayName;
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "EMail Sender Name [{0}] for E-Mail Id[{1}]", mailSenderName, mailSender);
                    }
                }
                retVal = true;
            }
            catch (Exception e)
            {
                this.logger.logException(function, e);
                retVal = false;
            }
            return retVal;
        }
        public bool sendSelectionMailWithResp(CvEvent eventDetailReq,UserInfo loggedInUser, MailServerDetails mailServerDetails, String smtpPassword,
            String subject, String mailBody, String mailFromDisplayName, String mailFrom, String imageName, String base64encodedImage, 
            bool isInlineImage, string senderSmtp, string senderName, bool isRequiredNotification, ref string errorMsg)
        {
            const string function = "CVWebSvc::sendSelectionMailWithResp";
            List<string> answers;
            String formattedMailBody = "";
            List<CvEventUser> participantsList = new List<CvEventUser>();
            CvEventReq eventReq = new CvEventReq();
            int sentMailCounter = 0;
            int totalRecipient = 0;
            bool retVal = false;
            long notParticipated = 0L;
            try
            {
                eventReq.events = new List<CvEvent>();
                eventReq.events.Add(eventDetailReq);
                eventReq.isOrganizerSearch = true;
                eventReq.participantWithResp = true;
                eventReq.getMailContent = false;
                eventReq.userInformation = loggedInUser;
                CvEventResp eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0, false);
                if (eventResp != null && eventResp.events != null && eventResp.events.Count > 0)
                {                  
                    CvEvent eventDetail = eventResp.events[0];
                    
                    Dictionary<Int32, Dictionary<Int32, string>> mapOfQuestionOptions = new Dictionary<Int32, Dictionary<Int32, string>>();
                    foreach (CvEventQuestion eachQues in eventDetail.eventQuestions)
                    {
                        Dictionary<Int32, string> quesOptMap = new Dictionary<Int32, string>();
                        if (eachQues.eventQuestionOptions != null)
                        {
                            foreach (CvEventQuestionOption eachQuesOpt in eachQues.eventQuestionOptions)
                                quesOptMap.Add(eachQuesOpt.eventQuestionOptionID, eachQuesOpt.eventOptionVal);
                            mapOfQuestionOptions.Add(eachQues.eventQuestionID, quesOptMap);
                        }
                    }
                    foreach (CvEventUser user in eventDetail.eventParticipants)
                    {
                        if (eventReq.participantWithResp)
                        {
                            if (user.participatedAt > notParticipated)
                            {
                                participantsList.Add(user);
                            }
                        }
                    }
                    Dictionary<Int32, string> quesOptMapList = new Dictionary<Int32, string>();
                    String ans = "";
                    totalRecipient = participantsList.Count;
                    //mapping the answers response with per user
                    //Sending mail with the mapped answers

                    //TODO : need to optimize this loop.
                    foreach (CvEventUser p in participantsList)
                    {
                        answers = new List<string>();
                        answers.Add(p.displayName);
                        foreach (CvEventQuestion eachQues in eventDetail.eventQuestions)
                        {
                            Int32 questId = eachQues.eventQuestionID;
                            ans = "";
                            if (mapOfQuestionOptions.ContainsKey(questId))
                            {
                                quesOptMapList = mapOfQuestionOptions[questId];
                            }
                            if (eachQues.eventQuestionResponse != null && eachQues.eventQuestionResponse.Count > 0)
                            {
                                foreach (CvEventResponse response in eachQues.eventQuestionResponse)
                                {
                                    if (p.displayName.Equals(response.participantName))
                                    {
                                        if (response.eventOptionResponse != null
                                                && response.eventOptionResponse.Count > 0)
                                        {
                                            if (quesOptMapList.ContainsKey(response.eventOptionResponse[0]))
                                            {
                                                ans += quesOptMapList[response.eventOptionResponse[0]] + ",";
                                            }
                                        }
                                        else
                                            ans = response.eventCustomResponse;
                                    }
                                }
                                if (ans.EndsWith(","))
                                    ans = ans.Substring(0, ans.Length - 1);
                                answers.Add(ans);
                            }
                            else
                            {
                                ans = "No response found";
                                answers.Add(ans);
                            }
                        }
                        answers.Add(p.userGuid);
                        formattedMailBody = CvEventsHelper.getFormattedMailBody(mailBody, answers.ToArray());
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Formatted email body [{0}]", formattedMailBody);
                        if (!sendMail(mailServerDetails, smtpPassword, p.smtpAddress,"", "", "", subject,
                            formattedMailBody, mailFromDisplayName, mailFrom, imageName, base64encodedImage,
                            true, senderSmtp, senderName, null, null, true, ref errorMsg))
                        {
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed for user [{0}] with error [{1}]", p.smtpAddress, errorMsg);
                        }
                        else
                            sentMailCounter++;
                    }
                    if (totalRecipient == sentMailCounter)
                    {
                        retVal = true;
                    }
                }
                else
                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "Event response is empty");

            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                retVal = false;
            }
            return retVal;
        }
        public Boolean sendMail(MailServerDetails mailServerDetails, String smtpPassword, String strMailTo,String mailToGUID, String MailBcc, String MailCc,
            String subject, String strBody, String mailFromDisplayName, String mailFrom, String imageName, String base64encodedImage, bool isInlineImage, string senderSmtp, string senderName, CalendarAttachment icsForRSVP,
            CalendarAttachment icsForResp, bool isRequiredNotification, ref string errorMsg)
        {
            const string function = "CVWebSvc::sendMail";
            int errCode = 0;
            bool retVal = false;
            Attachment icsForRSVPTOSend = null, icsForRespToSend = null;
            CalendarAttachment calObject = null;
            CvSMTPMgr cvsmtp = new CvSMTPMgr();
            try
            {
                cvsmtp.Init();
                logger.DebugLvl(function, LogLevel.LOG_VERBOSE, "Sending email to [{0}] subject [{1}] mailFrom[{2}] mailSender[{3}]", strMailTo, subject, mailFrom, senderSmtp);
                logger.DebugLvl(function, LogLevel.LOG_VERBOSE, "Email body [{0}]", strBody);

                if (strMailTo.Length != 0)
                {
                    cvsmtp.setTo(strMailTo);
                    logger.DebugLvl(function, LogLevel.LOG_VERBOSE, "Sending mail To Recipients as TO [{0}]", strMailTo);
                }
                if (MailBcc.Length != 0)
                {
                    cvsmtp.setBccList(MailBcc);
                    logger.DebugLvl(function, LogLevel.LOG_VERBOSE, "Sending mail To Recipients as BCC [{0}]", MailBcc);
                }
                if (MailCc.Length != 0)
                {
                    cvsmtp.setCcList(MailCc);
                    logger.DebugLvl(function, LogLevel.LOG_VERBOSE, "Sending mail To Recipients as CC [{0}]", MailCc);
                }
                if (icsForRSVP != null)
                {
                    calObject = new CalendarAttachment(icsForRSVP);
                    if (!String.IsNullOrEmpty(mailToGUID))
                    {
                        calObject.description = String.Format(calObject.description, mailToGUID);
                    }
                    icsForRSVPTOSend = Attachment.CreateAttachmentFromString(calObject.ToString(), new System.Net.Mime.ContentType("text/calendar"));
                    icsForRSVPTOSend.Name = calObject.name;
                    icsForRSVPTOSend.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                }
                if (icsForResp != null)
                {
                    calObject = new CalendarAttachment(icsForResp);
                    if (!String.IsNullOrEmpty(mailToGUID))
                    {
                        calObject.description = String.Format(calObject.description, mailToGUID);
                    }
                    icsForRespToSend = Attachment.CreateAttachmentFromString(calObject.ToString(), new System.Net.Mime.ContentType("text/calendar"));
                    icsForRespToSend.Name = calObject.name;
                    icsForRespToSend.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                }
                cvsmtp.SetParam(mailServerDetails.smtpServer, mailServerDetails.smtpPort, mailFromDisplayName, mailFrom, mailServerDetails.smtpUsername, mailServerDetails.smtpPassword,
                     "", subject, strBody, false, mailServerDetails.smtpssl, true, mailServerDetails.credRequired, imageName, true, base64encodedImage, isInlineImage, senderSmtp, senderName, icsForRSVPTOSend, icsForRespToSend, isRequiredNotification);
                errorMsg = cvsmtp.sendMessage(ref errCode);
                if (0 != errCode)
                {
                    logger.DebugLvl(function, LogLevel.LOG_ERROR, "sendMessage failed for user [{0}] with error [{2}]", strMailTo, errorMsg);
                    retVal = false;
                }
                else
                    retVal = true;
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                retVal = false;
            }
            return retVal;
        }
        

        private void AddUsersToEventReq(CvEventReq eventReq)
        {
            String function = "CvWebSvc::AddUsersToEventReq";
            if (eventReq != null && eventReq.events != null && eventReq.events.Count > 0)
            {
                try {                    

                    for (int eventCount = 0; eventCount < eventReq.events.Count; eventCount++)
                    {
                        eventReq.events[eventCount].eventParticipants = new List<CvEventUser>();
                        eventReq.events[eventCount].eventOrganizers = new List<CvEventUser>();
                        List<String> organizerMailIdList = new List<String>();
                        List<String> participantMailIdList = new List<String>();
                        CvEvent cvevent = eventReq.events[eventCount];
                        if (cvevent.eventOrganizersDisp != null && cvevent.eventOrganizersDisp.Count > 0)
                        {
                            foreach (CvEventUser organizer in cvevent.eventOrganizersDisp)
                            {
                                if (!string.IsNullOrWhiteSpace(organizer.smtpAddress))
                                {
                                    organizerMailIdList.Add(organizer.smtpAddress);
                                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "User :[{0}]", organizer.smtpAddress);
                                }
                            }

                        }

                        if(cvevent.eventParticipantsDisp != null && cvevent.eventParticipantsDisp.Count > 0)
                        {
                            foreach (CvEventUser participant in cvevent.eventParticipantsDisp)
                            {
                                if (!string.IsNullOrWhiteSpace(participant.smtpAddress))
                                {
                                    participantMailIdList.Add(participant.smtpAddress);
                                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "User :[{0}]", participant.smtpAddress);
                                }
                            }
                        }
                        //adding organizer mailids
                        if (organizerMailIdList != null && organizerMailIdList.Count > 0)
                        {
                            List<CvEventUser> organizers = new List<CvEventUser>();
                            organizers = checkIfGroupAndExpand(organizerMailIdList.ToArray());

                            for (int i = 0; i < organizers.Count; i++)
                            {
                                organizers[i].groupType = GroupType.NONE;
                            }

                            eventReq.events[eventCount].eventOrganizers.AddRange(organizers);

                            List<CvEventUser> adGroups = new List<CvEventUser>();
                            adGroups = checkAndGetADgroups(organizerMailIdList.ToArray());
                            if (adGroups != null && adGroups.Count > 0)
                            {
                                eventReq.events[eventCount].eventOrganizers.AddRange(adGroups);
                            }

                        }

                        //adding participants maillidlist
                        if (participantMailIdList != null && participantMailIdList.Count > 0)
                        {
                            List<CvEventUser> participants = new List<CvEventUser>();
                            participants = checkIfGroupAndExpand(participantMailIdList.ToArray());

                            for (int i = 0; i < participants.Count; i++)
                            {
                                participants[i].groupType = GroupType.NONE;
                            }

                            eventReq.events[eventCount].eventParticipants.AddRange(participants);

                            List<CvEventUser> adGroups1 = new List<CvEventUser>();
                            adGroups1 = checkAndGetADgroups(participantMailIdList.ToArray());
                            if (adGroups1 != null && adGroups1.Count > 0)
                            {
                                eventReq.events[eventCount].eventParticipants.AddRange(adGroups1);
                            }
                        }

                        /*foreach (CvEventUser user in eventReq.events[0].eventParticipants)
                            logger.DebugLvl(function, LogLevel.LOG_ERROR, "Users in Participant List :[{0}] [{1}] [{2}]", user.smtpAddress, user.userGuid, user.groupType);*/
                    }
                }
                     catch (Exception ex)
                     {
                        this.logger.logException(function, ex);
                     }

                }
            }

        public Message getInterestedParticipant(Message eventMsg)
        {
            const string function = "CVWebSvc::getInterestedParticipant";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventUsers userList = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, eventMsg);
            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().SearchEvents(eventReq, 0, 0, true);
                    if (eventResp != null && eventResp.events != null)
                    {
                        CvEvent cvEvent = eventResp.events[0];
                        if (cvEvent.eventParticipants != null)
                        {
                            userList = new CvEventUsers();
                            userList.users = cvEvent.eventParticipants;
                        }
                    }

                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.EVENT_REQUEST_EMPTY,
                        eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (userList == null)
                    {
                        userList = new CvEventUsers();
                    }
                    resp = CreateXMlTextMessage(function, userList);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        
        #region Group
        /*Group*/

        public Message CreateGroup(Message groupMsg)
        {
            const string function = "CVWebSvc::CreateGroup";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, groupMsg);
            try
            {
                if (eventReq != null)
                {
                    AddUsersToGroupReq(eventReq);
                    eventResp = new dmCvEvent().SaveGroup(eventReq);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_GROUP_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.GROUP_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message DeleteGroup(Message groupMsg)
        {
            const string function = "CVWebSvc::DeleteGroup";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = this.MessageToObject<CvEventReq>(function, groupMsg, false);
            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().DeleteGroup(eventReq);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_GROUP_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.GROUP_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = this.CreateXMlTextMessage(function, eventResp, false);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message GetTimelineGroups(Message groupMsg)
        {
            const string function = "CVWebSvc::GetTimelineGroups";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, groupMsg);

            try
            {
                if (eventReq != null)
                {
                    eventResp = new dmCvEvent().SearchGroups(eventReq);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_GROUP_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.GROUP_REQUEST_EMPTY, eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    if (eventResp.groups != null)
                    {
                        //logger.DebugLvl(function, LogLevel.LOG_ERROR, string.Format("Groups found [{0}]", eventResp.groups.Count));
                        //logger.DebugLvl(function, LogLevel.LOG_ERROR, string.Format("GroupAdmins found [{0}]", eventResp.groups[0].groupAdminsDisp[0].smtpAddress));    //remove later
                    }

                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        public Message EditGroup(Message groupMsg)
        {
            const string function = "CVWebSvc::EditGroup";
            Message resp = null;
            CvEventResp eventResp = null;
            Boolean isAddMore = false;
            CvEventReq eventReq = this.MessageToObject<CvEventReq>(function, groupMsg);
            try
            {
                if (eventReq != null)
                {
                    AddUsersToGroupReq(eventReq);
                    eventResp = new dmCvEvent().EditGroup(eventReq, isAddMore);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_GROUP_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.GROUP_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        private void AddUsersToGroupReq(CvEventReq eventReq)
        {
            const string function = "CvWebSvc::AddUsersToGroupReq";
            try
            {
                if (eventReq != null && eventReq.groups != null && eventReq.groups.Count > 0)
                {
                    for (int groupCount = 0; groupCount < eventReq.groups.Count; groupCount++)
                    {
                        //List<String> adminMailIdList = new List<String>();
                        List<String> memberMailIdList = new List<String>();
                        CvEventGroup cvEventGroup = eventReq.groups[groupCount];
                        if (cvEventGroup.groupUsers != null && cvEventGroup.groupUsers.Count > 0)
                        {
                            foreach (CvEventUser cvGroupUser in cvEventGroup.groupUsers)
                            {
                                //if (cvGroupUser.groupType == GroupType.NONE && cvGroupUser.userType == CvEventUserType.ADMIN)
                                //    adminMailIdList.Add(cvGroupUser.smtpAddress);
                                if (cvGroupUser.groupType == GroupType.NONE && cvGroupUser.userType == CvEventUserType.MEMBER)
                                    memberMailIdList.Add(cvGroupUser.smtpAddress);
                            }
                        }
                        
                        /* clearing existing groupUsers list */
                        eventReq.groups[groupCount].groupUsers.Clear();

                        //adding admin mailids
                        //if (adminMailIdList != null && adminMailIdList.Count > 0)
                        //{
                        //    List<CvEventUser> groupUsers =new List<CvEventUser>();
                        //    groupUsers=checkIfGroupAndExpand(adminMailIdList.ToArray());

                        //    for (int i = 0; i < groupUsers.Count; i++)
                        //    {
                        //        groupUsers[i].userType = CvEventUserType.ADMIN;
                        //        groupUsers[i].groupType = GroupType.NONE;
                        //        if (groupUsers[i].isDomainGroupMember!=true)
                        //            groupUsers[i].isDomainGroupMember = false;
                        //    }

                        //    eventReq.groups[groupCount].groupUsers.AddRange(groupUsers);

                        //    List<CvEventUser> adGroups = new List<CvEventUser>();
                        //    adGroups = checkAndGetADgroups(adminMailIdList.ToArray());
                        //    if (adGroups != null && adGroups.Count>0)
                        //    {
                        //        for (int i = 0; i < adGroups.Count; i++)
                        //        {
                        //            adGroups[i].userType = CvEventUserType.ADMIN;
                        //        }
                        //        eventReq.groups[groupCount].groupUsers.AddRange(adGroups);
                        //    }

                        //}

                        //adding member mailids
                        if (memberMailIdList != null && memberMailIdList.Count > 0)
                        {
                            List<CvEventUser> groupUsers = new List<CvEventUser>();
                            groupUsers = checkIfGroupAndExpand(memberMailIdList.ToArray());

                            for (int user = 0; user < groupUsers.Count; user++)
                            {
                                groupUsers[user].userType = CvEventUserType.MEMBER;
                                groupUsers[user].groupType = GroupType.NONE;
                                if (groupUsers[user].isDomainGroupMember != true)
                                    groupUsers[user].isDomainGroupMember = false;
                            }

                            eventReq.groups[groupCount].groupUsers.AddRange(groupUsers);

                            List<CvEventUser> adGroups = new List<CvEventUser>();
                            adGroups = checkAndGetADgroups(memberMailIdList.ToArray());
                            if (adGroups != null && adGroups.Count > 0)
                            {
                                for (int adGroup = 0; adGroup < adGroups.Count; adGroup++)
                                {
                                    adGroups[adGroup].userType = CvEventUserType.MEMBER;
                                }
                                eventReq.groups[groupCount].groupUsers.AddRange(adGroups);
                            }
                        }

                        //foreach(CvEventUser user in eventReq.groups[0].groupUsers)
                        //logger.DebugLvl(function, LogLevel.LOG_ERROR, "Users in :[{0}] [{1}] [{2}]",user.smtpAddress,user.userGuid,user.groupType);                   

                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
        }

        public Message AddMembers(Message groupMsg)
        {
            const string function = "CVWebSvc::AddMembers";
            Message resp = null;
            CvEventResp eventResp = null;
            CvEventReq eventReq = MessageToObject<CvEventReq>(function, groupMsg);
            try
            {
                if (eventReq != null)
                {
                    AddUsersToGroupReq(eventReq);
                    eventResp = new dmCvEvent().EditGroup(eventReq, true);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_GROUP_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.GROUP_REQUEST_EMPTY,
                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        /*Group*/

        #endregion

        #region Notifications
        /*Notifications*/

        public Message GetNotifications(Message userMsg)
        {
            const string function = "CVWebSvc::GetNotifications";
            Message resp = null;
            CvEventResp eventResp = null;

            CvEventReq eventReq = MessageToObject<CvEventReq>(function, userMsg);

            try
            {

                if(eventReq!=null && eventReq.userInformation!=null && !string.IsNullOrEmpty(eventReq.userInformation.userGuid))
                {
                    eventResp = new dmCvEvent().GetNotifications(eventReq.userInformation);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.USER_REQUEST_EMPTY, eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    resp = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return resp;
        }

        /*Notifications*/

        #endregion

        #region Contol User Profile
        /*Control user profile*/

        public Message ControlUserProfile(string userGuid, string modifierGuid, int eventId,int groupId,string tags,int isFavorite, int notificationCode, int status)
        {
            const string function="CVWebSvc::ControlUserProfile";
            GenericResp genResp=null;
            Message respMsg = null; ;

            CvEventReq eventReq = new CvEventReq();

            eventReq.userInformation = new UserInfo();
            eventReq.userInformation.userGuid = modifierGuid;

            CvEvent cvEvent=new CvEvent();            
            cvEvent.eventID = eventId;        

            //CvEventGroup cvGroup=new CvEventGroup();
            //cvGroup.groupId = groupId;

            

            eventReq.events = new List<CvEvent>();
            //eventReq.groups = new List<CvEventGroup>();

            eventReq.events.Add(cvEvent);
            //eventReq.groups.Add(cvGroup);

            try
            {
                if (eventReq != null)
                {
                    genResp = new dmCvEvent().ControlUserProfile(eventReq, userGuid,tags, isFavorite, notificationCode, status);
                }
                else
                {
                    Error err = GetEventError((int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.USER_REQUEST_EMPTY, CvEventExceptionStrings.EMPTY_EVENT_REQUEST);
                    genResp.errList.Add(err);
                }
            }
            catch (Exception ex){
                this.logger.logException(function, ex);
                genResp = new GenericResp();
                genResp.errList = new List<Error>();
                Error err = GetEventError((int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED, CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED);
                genResp.errList.Add(err);
            }
            finally
            {
                try
                {
                    if (genResp == null)
                    {
                        genResp = new GenericResp();
                    }
                    respMsg = CreateXMlTextMessage(function, genResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }
        /*control user profile*/

        #endregion

        #region Get User Information
        /*get user information*/
        public Message GetUserInformation(Message userInfo)
        {
            const string function = "function::GetUserInformation";
            CvEventResp eventResp = null;
            Message respMsg = null;

            UserInfo userInformation = MessageToObject<UserInfo>(function, userInfo);

            try
            {
                if(userInformation!=null&&!string.IsNullOrEmpty(userInformation.userGuid))
                {
                    CvEventUsers adGroups = new CvEventUsers();

                    adGroups = getADgroups(userInformation.smtpAddress);        //get adgroups of a user
                    eventResp = new dmCvEvent().GetUserInformation(userInformation,adGroups);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.USER_REQUEST_EMPTY, eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
                AddEventResponseError(CvEventExceptionStrings.UNEXPECTED_ERROR_OCCURRED,
                             (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.UNEXPECTED_ERROR_OCCURRED,
                             eventResp);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    respMsg = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }

        #endregion

        private Error GetEventError(int errCode, string errorMsg)
        {
            Error err = new Error();
            err.errorCode = errCode;
            err.errLogMessage = errorMsg;
            return err;
        }

        //Get all adgroups belongs to an user
        private CvEventUsers getADgroups(string userEmailId)
        {
            const string function = "CVWebSvc::getADgroups";
            
            string adPath = string.Empty, adHost = string.Empty;
            CvEventUser eventUser = new CvEventUser();
            List<CvEventUser> eventUsersList = new List<CvEventUser>();
            CvEventUsers eventUsers=new CvEventUsers();


            List<DM2ContentIndexingDataContract.DomainInfo> domainInfoList = getDomainInfoList();   //getting domains
            List<String> listOfGroups = new List<String>();
            string emailId = userEmailId;
            try
            {
                for (int i = 0; i < domainInfoList.Count; i++)
                {
                    DomainInfo domainInfo = domainInfoList[i];
                    adHost = domainInfo.hostName;
                    adPath = (domainInfo.secureLDAP ? "LDAPS://" : "LDAP://") + domainInfo.hostName;
                    String searchFilterForUser = "(&(objectClass=user)(mail={0}))";     //searching for user
                    String[] searchForUser = new String[] { "mail", "displayName", "objectGuid", "samaccountname", "memberOf", "distinguishedName" };   //properties to load

                    DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(adPath, domainInfo.serviceUserName,
                        DM2WebLib.dmConf.GetDecryptedPassword(domainInfo.servicePassword));     
                    DirectorySearcher searcher = new DirectorySearcher(entry);
                    searcher.Filter =(string.Format(searchFilterForUser, emailId)); //search filters
                    searcher.PropertiesToLoad.AddRange(searchForUser);
                    SearchResult result = searcher.FindOne();
                    if ((result != null))
                    {
                        int propertyCount = result.Properties["memberOf"].Count;    //get count of all adgroups in which user is a member
                        for (int p = 0; p < propertyCount; p++)
                        {
                            CvEventUser user = new CvEventUser();
                            string name = (string)result.Properties["memberOf"][p];     //get memberof object of user memberof contains(cn,ou,dc,dc...)
                            int startIndex = name.IndexOf("=") + 1;
                            user.displayName = name.Substring(startIndex, name.IndexOf(",") - startIndex);  //get the cn(common name) of group
                            user.smtpAddress = "";
                            //logger.DebugLvl(function, LogLevel.LOG_ERROR, "Group : [{0}]", user.displayName);
                            searcher.Filter = string.Format("(&(objectClass=group)(cn={0}))", user.displayName);    //search for group with its common name to get its guid
                            searcher.PropertiesToLoad.AddRange(searchForUser);
                            try
                            {
                                SearchResult result1 = searcher.FindOne();
                                if (result1 != null)
                                {
                                    int count = result1.Properties["mail"].Count;
                                    for (int j = 0; j < count; j++)
                                    {
                                        //user.smtpAddress = (string)result1.Properties["mail"][j];       //get the mail id of the group
                                        byte[] objectGuid = (byte[])result1.Properties["objectGuid"][j];
                                        user.groupGuid = new Guid(objectGuid).ToString();       //get the guid of the group
                                    }
                                    //logger.DebugLvl(function, LogLevel.LOG_ERROR, "GroupMail : [{0}]", user.smtpAddress);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
                            }
                            if (!string.IsNullOrEmpty(user.groupGuid))
                            eventUsersList.Add(user);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
            }
            finally
            {
                try
                {
                    if (eventUsersList == null)
                        eventUsersList = new List<CvEventUser>();
                    eventUsers.users = eventUsersList;
                }
                catch(Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return eventUsers;
        }

        private List<CvEventUser> checkAndGetADgroups(string[] EmailIds)
        {
            const string function = "CVWebSvc::checkAndGetADgroups";

            string adPath = string.Empty;
            CvEventUser eventUser = new CvEventUser();
            List<CvEventUser> eventUsersList = new List<CvEventUser>();
            List<CvEventUser> adGroupsList = new List<CvEventUser>();

            List<DM2ContentIndexingDataContract.DomainInfo> domainInfoList = getDomainInfoList();
            try
            {
                foreach (string emailId in EmailIds)
                {
                    for (int j = 0; j < domainInfoList.Count; j++)
                    {
                        DomainInfo domainInfo = domainInfoList[j];
                        adPath = (domainInfo.secureLDAP ? "LDAPS://" : "LDAP://") + domainInfo.hostName;
                        String searchFilterForGroup = "(&(objectClass=group)(mail={0}))";   //search for group with mail id 
                        String[] searchForGroup = new String[] { "mail", "displayName", "objectGuid", "samaccountname", "memberOf" };

                        DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(adPath, domainInfo.serviceUserName,
                            DM2WebLib.dmConf.GetDecryptedPassword(domainInfo.servicePassword));
                        DirectorySearcher searcher = new DirectorySearcher(entry);
                        searcher.Filter= (string.Format(searchFilterForGroup, emailId));
                        searcher.PropertiesToLoad.AddRange(searchForGroup);
                        SearchResultCollection result = searcher.FindAll();
                        if (result != null)
                        {
                            eventUsersList = addUserListInCvEventUser(result);  //get the group properties like guid,mail in eventUsersList
                            break;
                        }
                    }
                    if (eventUsersList != null && eventUsersList.Count > 0)
                    {
                        eventUsersList[0].groupType = GroupType.DOMAIN_GROUP;   //set the groupType as ADgroup
                        eventUsersList[0].groupGuid = eventUsersList[0].userGuid;
                        logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Mail Id :[{0}] belongs to group [{1}]", eventUsersList[0].smtpAddress, eventUsersList[0].groupGuid);
                        adGroupsList.Add(eventUsersList[0]);    //add group to groupList
                    }
                }
            }
            catch (Exception ex)
            {
                logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, "Exception occurred: [{0}]. Stack Trace:[{1}]", ex.Message, ex.StackTrace);
            }
            
            return adGroupsList;            
        }


        public Message checkIfGroupExists(string groupName)
        {
            const string function = "CvWebSvc::checkIfGroupExists";

            CvEventGroup groupResp = null;
            Message respMsg = null;

            try
            {
                if (groupName != null)
                {
                    groupResp = new dmCvEvent().checkIfGroupExists(groupName);
                }
              
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {
                try
                {
                    if (groupResp == null)
                    {
                        groupResp = new CvEventGroup();
                    }
                    respMsg = CreateXMlTextMessage(function, groupResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }

        public Message getGroupSuggestions(string groupName,int pageSize)
        {
            const string function = "CvWebSvc::getGroupSuggestions";            
            CvEventGroups groupResp = null;
            Message respMsg = null;
            try
            {
                if (groupName != null)
                {
                    logger.DebugLvl(function, LogLevel.LOG_DIAGNOSE, string.Format("Sending request {0}", groupName)); //to be removed
                    groupResp = new dmCvEvent().getGroupSuggestions(groupName,pageSize);
                }

            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {
                try
                {
                    if (groupResp == null)
                    {
                        groupResp = new CvEventGroups();
                    }
                    respMsg = CreateXMlTextMessage(function, groupResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }


        public Message getUserAndGroupSuggestions(string userNamePart, int pageSize)
        {
            const string function = "CvWebSvc::getUserAndGroupSuggestions";
            Message respMsg = null;
            Message userSuggestions = null;
            Message groupSuggestions = null;
            CvEventResp eventResp = new CvEventResp();
            CvEventUsers cvUsers=new CvEventUsers();
            CvEventGroups cvGroups=new CvEventGroups();
            try
            {
                userSuggestions = GetUserSuggestions(userNamePart, pageSize);
                groupSuggestions = getGroupSuggestions(userNamePart, pageSize);
                if(userSuggestions!=null)
                cvUsers = MessageToObject<CvEventUsers>(function, userSuggestions);

                if (groupSuggestions != null)
                    cvGroups = MessageToObject<CvEventGroups>(function, groupSuggestions);

                if(cvUsers!=null&&cvUsers.users!=null)
                eventResp.users = cvUsers.users;

                if (cvGroups != null && cvGroups.groups != null)
                    eventResp.groups = cvGroups.groups;
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    respMsg = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }

        public Message getTagSuggestions(string pattern, int pageSize)
        {
            const string function = "CvWebSvc::getTagSuggestions";
            Message respMsg = null;
            CvEventResp eventResp = new CvEventResp();

            try
            {                        
                    eventResp = new dmCvEvent().getTagSuggestions(pattern, pageSize);
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    respMsg = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }

        public Message getUserInterest(Message userInfo)
        {
            const string function = "CvWebSvc::getUserInterest";
            Message respMsg = null;
            CvEventResp eventResp = new CvEventResp();
            UserInfo userInformation = MessageToObject<UserInfo>(function, userInfo);

            try
            {
                if (userInformation != null && !string.IsNullOrEmpty(userInformation.userGuid))
                {
                    eventResp = new dmCvEvent().getUserInterest(userInformation);
                }
                else
                {
                    AddEventResponseError(CvEventExceptionStrings.EMPTY_EVENT_REQUEST, (int)ErrorMessageFramework.ErrorCode.ErrorCodeConstants.CVEVENTS_ERROR_CODES.USER_GUID_NOT_PROVIDED,
                                       eventResp, function, true);
                }
            }
            catch (Exception ex)
            {
                this.logger.logException(function, ex);
            }
            finally
            {
                try
                {
                    if (eventResp == null)
                    {
                        eventResp = new CvEventResp();
                    }
                    respMsg = CreateXMlTextMessage(function, eventResp);
                }
                catch (Exception ex)
                {
                    this.logger.logException(function, ex);
                }
            }
            return respMsg;
        }

    }

}

    