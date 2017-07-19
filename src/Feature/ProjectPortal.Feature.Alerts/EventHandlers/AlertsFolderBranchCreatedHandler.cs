using System;
using System.Linq;
using Glass.Mapper.Sc;
using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Pipelines.ItemProvider.AddFromTemplate;
using Assert = Sitecore.Diagnostics.Assert;

namespace ProjectPortal.Feature.Alerts.EventHandlers
{
    public class AlertsFolderBranchCreatedHandler : AddFromTemplateProcessor
    {
        protected void OnItemCreated(object sender, EventArgs args)
        {
            var createdArgs = Event.ExtractParameter<ItemCreatedEventArgs>(args, 0);
            var item = createdArgs.Item;

            //If this is not in the master database then return
            if (!item.Database.Name.Equals("master")) return;

            if (item.Parent.TemplateID.ToString().Equals(AlertTempalteIds.AlertsFolder))
            {
                if (item.TemplateID.ToString().Equals(AlertTempalteIds.SavedSearches))
                {
                    try
                    {
                        item.Editing.BeginEdit();
                        item.Fields[Sitecore.Buckets.Util.Constants.DefaultQuery].SetValue(
                            "location:{0a6536b7-afe8-4121-ae74-ef7501e36491};template:{d9577b16-776f-48c3-b2f8-e31514acd492};custom:IsActive|true",
                            true);
                        //item.Fields.ReadAll();
                        //foreach (var field in item.Fields)
                        //{
                        //    Console.WriteLine(field);
                        //}
                        item.Editing.EndEdit(false, false);
                    }
                    catch (Exception e)
                    {
                        item.Editing.CancelEdit();
                    }
                    finally
                    {
                        item.Editing.EndEdit(false, false);
                    }
                    //Change the location folder
                }
            }
        }


        public override void Process(AddFromTemplateArgs args)
        {
            //System.Diagnostics.Debugger.Launch();
            //System.Diagnostics.Debugger.Break();
            if (args.Aborted)
            {
                return;
            }

            Assert.IsNotNull(args.FallbackProvider, "FallbackProvider is null");

            try
            {
                var item = args.FallbackProvider.AddFromTemplate(args.ItemName, args.TemplateId, args.Destination, args.NewId);
                if (item == null)
                {
                    return;
                }

                if (item.TemplateID.ToString().Equals(AlertTempalteIds.AlertsFolder))
                {
                    var locationId = item.ID.Guid.ToString();

                    //Get the saved search
                    var savedSearches =
                        item.Children.Where(child => child.TemplateID.ToString().Equals(AlertTempalteIds.SavedSearches));

                    var activeAlertsQueryFilter =
                        $"+location:{locationId};+template:{AlertTempalteIds.AlertDataSource};+custom:{FieldNames.IsActive}|true";

                    var inActiveAlertsQueryFilter =
                        $"+location:{locationId};+template:{AlertTempalteIds.AlertDataSource};+custom:{FieldNames.IsActive}|false";

                    foreach (var itemChild in savedSearches)
                    {
                        using (new EditContext(itemChild, true, true))
                        {
                            if (itemChild.DisplayName.Equals(ItemNames.ActiveAlertsTitle))
                            {
                                itemChild.Fields[Sitecore.Buckets.Util.Constants.DefaultQuery].SetValue(activeAlertsQueryFilter, true);
                            }
                            else if (itemChild.DisplayName.Equals(ItemNames.InActiveAlertsTitle))
                            {
                                itemChild.Fields[Sitecore.Buckets.Util.Constants.DefaultQuery].SetValue(inActiveAlertsQueryFilter, true);
                            }
                        }
                    }
                }

                args.ProcessorItem = args.Result = item;
            }
            catch (Exception ex)
            {
                var item = args.Destination.Database.GetItem(args.NewId);
                item?.Delete();

                throw;
            }

            // your logic here
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            var item = Event.ExtractParameter<Item>(args, 0);

            //If this is not in the master database then return
            if (!item.Database.Name.Equals("master")) return;

            if (item.Parent.TemplateID.ToString().Equals(AlertTempalteIds.AlertsFolder))
            {
                if (item.TemplateID.ToString().Equals(AlertTempalteIds.SavedSearches))
                {
                    try
                    {
                        //This is the final query to use: 
                        //+location:{0a6536b7-afe8-4121-ae74-ef7501e36491};+template:{d9577b16-776f-48c3-b2f8-e31514acd492};+custom:IsActive|true                        
                        item.Editing.BeginEdit();
                        item.Fields[Sitecore.Buckets.Util.Constants.DefaultQuery].SetValue(
                            "location:{0a6536b7-afe8-4121-ae74-ef7501e36491};template:{d9577b16-776f-48c3-b2f8-e31514acd492};custom:IsActive|true",
                            true);
                        item.Editing.EndEdit(false, true);
                    }
                    catch (Exception e)
                    {
                        item.Editing.CancelEdit();
                    }
                    finally
                    {
                        item.Editing.EndEdit(false, true);
                    }
                    //Change the location folder
                }
            }
        }
    }
}