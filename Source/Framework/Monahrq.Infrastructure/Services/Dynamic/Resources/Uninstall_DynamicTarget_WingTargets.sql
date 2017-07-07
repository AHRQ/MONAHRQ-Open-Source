/*
 * Uninstall dynamic wing targets and data
 */
 update [dbo].[Wings_Elements] SET [Scope_id] = null WHERE [Target_Id] = [@@DynamicTargetId@@];
 delete from [dbo].[Wings_ScopeValues] where [Scope_Id] in ( select distinct s.[Id] from [dbo].[Wings_Scopes] s where s.[Target_Id] = [@@DynamicTargetId@@] );
 delete from [dbo].[Wings_Scopes] where [Target_Id] = [@@DynamicTargetId@@];
 delete from [dbo].[Wings_Elements] where [Target_Id] =  [@@DynamicTargetId@@];
 delete from [dbo].[Wings_Targets] where [Id] =  [@@DynamicTargetId@@];