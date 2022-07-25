// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace FocusCenterPRChecker.Models
{
    public class CommentJsonModel
    {
        public Comment[] Comments { get; set; }
        public int Status { get; set; }


    }

    public class Comment
    {
        public string Content { get; set; }
        public int ParentCommentId { get; set; }
        public int CommentType { get; set; }
    }
}
