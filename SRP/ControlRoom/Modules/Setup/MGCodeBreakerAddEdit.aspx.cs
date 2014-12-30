﻿using System;
using System.Web.UI.WebControls;
using SRPApp.Classes;
using STG.SRP.ControlRooms;
using STG.SRP.Core.Utilities;
using STG.SRP.DAL;
using STG.SRP.Utilities.CoreClasses;

namespace STG.SRP.ControlRoom.Modules.Setup
{
    public partial class MGCodeBreakerAddEdit : BaseControlRoomPage
    {


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetPageRibbon(StandardModuleRibbons.SetupRibbon());
            }

            //MasterPage.RequiredPermission = PERMISSIONID;
            MasterPage.IsSecure = true;
            MasterPage.PageTitle = string.Format("{0}", "Code Breaker Game Edit");

            if (!IsPostBack)
            {
                lblPK.Text = Request["PK"];
                if (lblPK.Text.Length == 0)
                {
                    dv.ChangeMode(DetailsViewMode.Insert);
                }
                else
                {
                    dv.ChangeMode(DetailsViewMode.Edit);
                }
                Page.DataBind();
            }
        }

        public string GetCodedString(string s, int CBID)
        {

            string codedString = "";


            foreach (char c in s.ToCharArray())
            {
                if (c != ' ')
                {
                    codedString = codedString + "<img src='/Images/Games/CodeBreaker/" + CBID.ToString() + "_" +
                                  ((int) c).ToString() + ".png?" + DateTime.Now + "' border='1'/> " ;
                }
                else
                {
                    codedString = codedString + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                }
            }


            return codedString;
        }


        protected void dv_DataBound(object sender, EventArgs e)
        {
            if (dv.CurrentMode == DetailsViewMode.Edit)
            {
                var ctl = (DropDownList)dv.FindControl("AwardedBadgeID"); //this.FindControlRecursive(this, "BranchId");
                var lbl = (Label)dv.FindControl("AwardedBadgeIDLbl");
                var i = ctl.Items.FindByValue(lbl.Text);
                if (i != null) ctl.SelectedValue = lbl.Text;

                var control = (STG.SRP.Classes.FileDownloadCtl)dv.FindControl("FileUploadCtl");
                if (control != null) control.ProcessRender();
 
            }
        }

        protected void dv_DataBinding(object sender, EventArgs e)
        {
        }


        protected void DvItemCommand(object sender, DetailsViewCommandEventArgs e)
        {
            string returnURL = "~/ControlRoom/Modules/Setup/MiniGameList.aspx";

            if (e.CommandName.ToLower() == "more")
            {
                Response.Redirect("~/ControlRoom/Modules/Setup/MGCodeBreakerKeySetup.aspx?MGID=" + e.CommandArgument + "&CBID=" + ((TextBox)((DetailsView)sender).FindControl("CBID")).Text);
            }

            if (e.CommandName.ToLower() == "preview")
            {
                Session["MGID"] = e.CommandArgument;
                int key = Convert.ToInt32(e.CommandArgument);
                var obj = Minigame.FetchObject(key);
                Session["CRGoToUrl"] = Minigame.GetEditPage(obj.MiniGameType) + "?PK=" + e.CommandArgument;
                Response.Redirect("~/ControlRoom/Modules/Setup/MinigamePreview.aspx?MGID=" + e.CommandArgument);
            }

            if (e.CommandName.ToLower() == "back")
            {
                Response.Redirect(returnURL);
            }
            if (e.CommandName.ToLower() == "refresh")
            {
                try
                {
                    odsData.DataBind();
                    dv.DataBind();
                    dv.ChangeMode(DetailsViewMode.Edit);

                    var masterPage = (IControlRoomMaster)Master;
                    if (masterPage != null) masterPage.PageMessage = SRPResources.RefreshOK;
                }
                catch (Exception ex)
                {
                    var masterPage = (IControlRoomMaster)Master;
                    masterPage.PageError = String.Format(SRPResources.ApplicationError1, ex.Message);
                }
            }

            if (e.CommandName.ToLower() == "save" || e.CommandName.ToLower() == "saveandback")
            {
                try
                {
                    var obj = new MGCodeBreaker();
                    //int pk = int.Parse(((DetailsView)sender).Rows[0].Cells[1].Text);
                    int pk = int.Parse(((TextBox)((DetailsView)sender).FindControl("CBID")).Text);
                    obj.Fetch(pk);

                    var obj2 = Minigame.FetchObject(obj.MGID);


                    obj2.AdminName = ((TextBox)((DetailsView)sender).FindControl("AdminName")).Text;
                    obj2.GameName = ((TextBox)((DetailsView)sender).FindControl("GameName")).Text;
                    obj2.isActive = ((CheckBox)((DetailsView)sender).FindControl("isActive")).Checked;
                    obj2.NumberPoints = FormatHelper.SafeToInt(((TextBox)((DetailsView)sender).FindControl("NumberPoints")).Text);
                    obj2.AwardedBadgeID = FormatHelper.SafeToInt(((DropDownList)((DetailsView)sender).FindControl("AwardedBadgeID")).SelectedValue);
                    obj2.Acknowledgements = ((CKEditor.NET.CKEditorControl)((DetailsView)sender).FindControl("Acknowledgements")).Text;

                    obj.EasyString = ((TextBox)((DetailsView)sender).FindControl("EasyString")).Text;
                    obj.MediumString = ((TextBox)((DetailsView)sender).FindControl("MediumString")).Text;
                    obj.HardString = ((TextBox)((DetailsView)sender).FindControl("HardString")).Text;

                    obj.EnableMediumDifficulty = ((CheckBox)((DetailsView)sender).FindControl("EnableMediumDifficulty")).Checked;
                    obj.EnableHardDifficulty = ((CheckBox)((DetailsView)sender).FindControl("EnableHardDifficulty")).Checked;

                    obj2.LastModDate = obj.LastModDate = DateTime.Now;
                    obj2.LastModUser = obj.LastModUser = ((SRPUser)Session[SessionData.UserProfile.ToString()]).Username;  //"N/A";  // Get from session

                    if (obj.IsValid(BusinessRulesValidationMode.UPDATE) && obj2.IsValid(BusinessRulesValidationMode.UPDATE))
                    {
                        obj.Update();
                        obj2.Update();
                        if (e.CommandName.ToLower() == "saveandback")
                        {
                            Response.Redirect(returnURL);
                        }

                        odsData.DataBind();
                        dv.DataBind();
                        dv.ChangeMode(DetailsViewMode.Edit);

                        var masterPage = (IControlRoomMaster)Master;
                        masterPage.PageMessage = SRPResources.SaveOK;
                    }
                    else
                    {
                        var masterPage = (IControlRoomMaster)Master;
                        string message = String.Format(SRPResources.ApplicationError1, "<ul>");
                        foreach (BusinessRulesValidationMessage m in obj.ErrorCodes)
                        {
                            message = string.Format(String.Format("{0}<li>{{0}}</li>", message), m.ErrorMessage);
                        }
                        message = string.Format("{0}</ul>", message);
                        masterPage.PageError = message;
                    }
                }
                catch (Exception ex)
                {
                    var masterPage = (IControlRoomMaster)Master;
                    masterPage.PageError = String.Format(SRPResources.ApplicationError1, ex.Message);
                }
            }
        }
    }
}
