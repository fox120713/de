using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;
using System.Text;
using System.IO;
using PagedList;
namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/

        YIYUNEntities15 db = new YIYUNEntities15();
        static StringBuilder sql = new StringBuilder();
        #region 用户
        #region   母版页跳转
        /// <summary>
        /// 搜索详情页
        /// </summary>
        /// <returns></returns>

        public ActionResult cx()
        {
            return View();
        }
        [HttpPost]
        public ActionResult cx(string gqname)
        {
            Session["gqname"] = gqname;
            return View();
        }
        public ActionResult cxx(int page, int limit)
        {
            string s = Session["gqname"].ToString();
            //歌单
            var ged = from a in db.Songlist
                      join b in db.Songlistss on a.gdid equals b.gdid
                      join c in db.Song on b.gqid equals c.gqid
                      join d in db.Zhuanj on c.zjid equals d.zjid
                      join e in db.Singer on c.gsid equals e.gsid
                      join f in db.SongRD on c.gqid equals f.gqid
                      select new LayuiJson
                      {
                          gqid = c.gqid,
                          gsid = d.gsid,
                          gq = c.gqname,
                          zjname = d.zjname,
                          gsname = e.gsname,
                          gqimg = c.gqimg,
                          gqhot = f.gqhot
                      } into gd
                      select gd;
            LayuiJson layuijson = new LayuiJson();
            layuijson.code = 0;
            layuijson.msg = "";
            layuijson.count = ged.Count();//此为数据的总数，此处作者是随意编写的，在实际过程中是需要你们根据你所查询到数据的数量所填写的
            ged = ged.Where(md => md.gq.Contains(s));
            var page_list = ged.OrderByDescending(md => md.gqhot).Skip((page - 1) * limit).Take(limit).Distinct().ToList<object>();
            layuijson.data = page_list;
            return Json(layuijson, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 我的音乐详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult MYgedan()
        {
            int userid = Convert.ToInt32(Session["userid"]);

            if (userid == 0)
            {
                Response.Write(" <script>alert('请您先登录！')</script>");
                gxtj();
                return View("gxtj");
            }
            else
            {
                var item = from a in db.Songlist
                           join d in db.Users
                                  on a.userid equals d.userid
                           select new MYsonglist
                           {
                               gdid = a.gdid,
                               gdname = a.gdname,
                               gdimg = a.gdimg,
                               userid = d.userid,
                           } into ss
                           select ss;
                var item1 = item.Where(p => p.userid == userid).Distinct().ToList();

                ViewData["Mygedan"] = item1;
                return View();

            }

        }
        public ActionResult MYgedancx(int gdid)
        {
            Session["MYgedid"] = gdid;
            //右边
            //歌曲列表linq语句连表查询       
            var newgxtj = from a in db.Songlist
                          join b in db.Users on a.userid equals b.userid
                          select new remtjtiaozhuan
                          {
                              gdimg = a.gdimg,
                              userimg = b.usersimg,
                              username = b.username,
                              gdname = a.gdname,
                              gdid = a.gdid
                          } into gxtj
                          select gxtj;
            var newzjtj1 = newgxtj.Where(p => p.gdid == gdid).ToList();
            ViewData["remtj"] = newzjtj1;
            //歌单
            var ged = from a in db.Songlist
                      join b in db.Songlistss on a.gdid equals b.gdid
                      join c in db.Song on b.gqid equals c.gqid
                      join d in db.Zhuanj on c.zjid equals d.zjid
                      join e in db.Singer on c.gsid equals e.gsid
                      select new gedansong
                      {

                          gqid = c.gqid,
                          gdid = a.gdid,
                          gq = c.gqname,
                          mp3 = c.mp3,
                          zjname = d.zjname,
                          gsname = e.gsname
                      } into gd
                      select gd;
            var ged1 = ged.Where(p => p.gdid == gdid).ToList();
            ViewData["ged"] = ged1;
            return View();
        }

        public ActionResult MYgedanxz(HttpPostedFileBase file, string gdname)
        {
            Songlist newgd = new Songlist();
            string iamg = Server.MapPath("/gedanimg/" + file.FileName);
            file.SaveAs(iamg);
            int userid = Convert.ToInt32(Session["userid"]);
            newgd.gdname = gdname;
            newgd.gdimg = file.FileName;
            newgd.userid = userid;
            db.Songlist.Add(newgd);
            db.SaveChanges();
            MYgedan();
            return View("MYgedan");
        }

        public ActionResult MYgedandele(int gdid)
        {
            var id = db.Songlist.FirstOrDefault(p => p.gdid == gdid);
            db.Songlist.Remove(id);
            db.SaveChanges();
            MYgedan();
            return View("MYgedan");
        }

        public ActionResult MYgedandelegq(int gqid)
        {
            var gedid = Convert.ToInt32(Session["MYgedid"]);
            var rd = db.SongRD.FirstOrDefault(p => p.gqid == gqid);
            rd.gqhot -= 1;
            var id = db.Songlistss.FirstOrDefault(p => p.gqid == gqid);
            var pd = db.Songlistss.Where(p => p.gdid == gedid).ToList();
            if (pd.Count() > 1)
            {
                db.Songlistss.Remove(id);
            }
            else
            {
                Response.Write(" <script>alert('给歌单留一首歌吧！')</script>");
            }
            db.SaveChanges();
            MYgedan();
            return View("MYgedan");
        }
        #endregion

        #region   用户界面

        #region   登录模块
        public ActionResult fhsy()
        {
            Session["id"] = 0;
            Session["userid"] = 0;
            gxtj();
            return View("gxtj");
        }
        /// <summary>
        /// 用户界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="xz"></param>
        /// <returns></returns>
        public ActionResult denglu()
        {
            var s = Session["id"];

            return View(s);

        }
        [HttpPost]
        public ActionResult denglu(string username, int userpwd)
        {
            StringBuilder sb = new StringBuilder();
            Users user = new Users();
            var li = db.Users.Where(p => p.username == username && p.userpwd == userpwd).ToList();
            if (li.Count > 0)
            {
                //用户账号名传值
                Session["yhname"] = username;
                //用户头像传值
                Session["yhimg"] = "/usersimg/" + li.ElementAt(0).usersimg;

                int userid = li[0].userid;

                Session["id"] = 1;
                //身份管理员
                var gly = db.Users.Where(p => p.gly == 1 && p.username == username).ToList();
                if (gly.Count > 0)
                {
                    //用户账号名传值
                    Session["yhname"] = username;
                    //用户头像传值
                    Session["yhimg"] = "/userimg/" + li.ElementAt(0).usersimg;
                    System.Web.HttpContext.Current.Session["userid"] = db.Users.FirstOrDefault(p => p.username == username).userid;
                    Response.Write(" <script>alert('欢迎您亲爱的管理员！')</script>");
                    return View("guanli");
                }
                //身份用户(普通用户：VIP用户)
                else
                {

                    //用户账号名传值
                    Session["yhname"] = username;
                    //用户头像传值
                    Session["yhimg"] = "/userimg/" + li.ElementAt(0).usersimg;

                    //身份vip
                    var vip = db.Users.Where(p => p.vip == 1 && p.username == username).ToList();
                    if (vip.Count > 0)
                    {
                        Response.Write(" <script>alert('您已成功登录！')</script>");
                        Session["userid"] = userid;
                        System.Web.HttpContext.Current.Session["userid"] = db.Users.FirstOrDefault(p => p.username == username).userid;
                        gxtj();
                        return View("gxtj");
                    }
                    //身份普通用户
                    else
                    {
                        Response.Write(" <script>alert('您已成功登录！')</script>");
                        Session["userid"] = userid;
                        System.Web.HttpContext.Current.Session["userid"] = db.Users.FirstOrDefault(p => p.username == username).userid;
                        gxtj();
                        return View("gxtj");
                    }

                }

            }

            else
            {
                Response.Write(" <script>alert('登录失败')</script>");
                return View("denglu");
            }

        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        public ActionResult zhuce(string newusername, string newuserpwd, string x)
        {
            Users user = new Users();
            user.username = newusername;
            user.userpwd = Convert.ToInt32(newuserpwd);
            user.sex = x;
            user.age = 18;
            user.vip = 0;
            user.gly = 0;
            user.brith = DateTime.Now;

            var itemname = db.Users.Where(p => p.username == newusername).ToList();
            if (newusername == "" || newuserpwd == "")
            {
                Response.Write(" <script>alert('用户名和密码不能为空！')</script>");
                return View("denglu");
            }
            else
            {
                if (itemname.Count > 0)
                {
                    Response.Write(" <script>alert('该用户已存在，请重新注册')</script>");
                    return View("denglu");
                }
                else
                {

                    db.Users.Add(user);
                    db.SaveChanges();
                    Response.Write(" <script>alert('注册成功！请登录')</script>");
                    return View("denglu");
                }
            }

        }
        #endregion

        #region   功能模块
        /// <summary>
        /// 个性推荐
        /// </summary>
        /// <returns></returns>
        public ActionResult gxtj()
        {

            //热门推荐linq连表查询
            var newrmtj = from a in db.Song
                          join b in db.SongRD on a.gqid equals b.gqid
                          join c in db.Songlistss on a.gqid equals c.gqid
                          join d in db.Songlist on c.gdid equals d.gdid

                          select new newrmtj
                          {
                              gdname = d.gdname,
                              gdid = d.gdid,
                              gqid = a.gqid,
                              gdimg = d.gdimg,
                              js = a.js,
                              gqhot = b.gqhot
                          } into rmtj
                          select rmtj;
            //根据歌曲的热度进行降序排序.Take（4）相当与sql语句中的top（4）
            var newrmtj1 = newrmtj.OrderByDescending(b => new { b.gqhot }).Take(4).ToList();
            ViewData["rmtj"] = newrmtj1;
            //专辑推荐linq连表查询
            var newzjtj = from a in db.Zhuanj
                          select new newzjtj
                          {
                              zjid = a.zjid,
                              img = a.Zhuanjimg,
                              zjname = a.zjname,
                              zjhot = a.zjhot,

                          } into zjtj
                          select zjtj;
            //根据专辑的热度进行降序排序.Take（4）相当与sql语句中的top（4）
            var newzjtj1 = newzjtj.OrderByDescending(a => new { a.zjhot }).Take(4).ToList();
            ViewData["zjtj"] = newzjtj1;
            //名气歌手linq连表查询
            var newmqges = from a in db.Singer
                           join b in db.SingerRD
                           on a.gsid equals b.gsid
                           select new newmqges
                           {
                               gsid = a.gsid,
                               gsimg = a.gsimg,
                               gsname = a.gsname,
                               gqhot = b.gshot
                           } into mqges
                           select mqges;
            //根据歌手的热度进行降序排序.Take（4）相当与sql语句中的top（4）
            var newmqges1 = newmqges.OrderByDescending(a => new { a.gqhot }).Take(4).ToList();
            ViewData["mqges"] = newmqges1;
            //歌曲排行榜linq连表查询
            var newgeqpaih = from a in db.Song
                             join b in db.Singer on a.gsid equals b.gsid
                             join c in db.Zhuanj on a.zjid equals c.zjid
                             join d in db.SongRD on a.gqid equals d.gqid
                             select new newgeqpaih
                             {
                                 gqid = a.gqid,
                                 gsname = b.gsname,
                                 zjname = c.zjname,
                                 gq = a.gqname,
                                 gqhot = d.gqhot,
                                 mp3 = a.mp3
                             } into geqpaih
                             select geqpaih;
            //根据歌手的热度进行降序排序.Take（4）相当与sql语句中的top（4）
            var newgeqpaih1 = newgeqpaih.OrderByDescending(a => new { a.gqhot }).Take(20).ToList();
            ViewData["geqpaih"] = newgeqpaih1;
            return View();

        }

        /// <summary>
        /// 歌单
        /// </summary>
        /// <returns></returns>
        public ActionResult gdye()
        {
            var item = db.Songlist.ToList();

            return View(item);
        }
        /// <summary>
        /// 排行榜
        /// </summary>
        /// <returns></returns>

        public ActionResult phb(int? page)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                sql.Clear();
                sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid join Songtype on Songtype.typeid=Song.typeid join SongRD on Song.gqid=SongRD.gqid order by gqhot desc");
                var list = db.Database.SqlQuery<gs_gq_rd>(sql.ToString()).ToList();
                Session["song_number"] = list.Count();
                int pageNumber = page ?? 1;
                int pageSize = 15;
                IPagedList<gs_gq_rd> userPagedList = list.ToPagedList(pageNumber, pageSize);
                return View(userPagedList);
            }
        }
        /// <summary>
        /// 排行榜替换
        /// </summary>
        /// <param name="page"></param>
        /// <param name="bdid"></param>
        /// <returns></returns>
        public ActionResult phb_table(int? page, int bdid)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                if (bdid == 1)
                {
                    //热度榜
                    sql.Clear();
                    sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid join Songtype on Songtype.typeid=Song.typeid join SongRD on Song.gqid=SongRD.gqid order by gqhot desc");
                    var list = db.Database.SqlQuery<gs_gq_rd>(sql.ToString()).ToList();
                    Session["song_number"] = list.Count();
                    int pageNumber = page ?? 1;
                    int pageSize = 15;
                    IPagedList<gs_gq_rd> userPagedList = list.ToPagedList(pageNumber, pageSize);
                    return View(userPagedList);
                }
                if (bdid == 2)
                {
                    //新歌榜
                    sql.Clear();
                    sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid join Songtype on Songtype.typeid=Song.typeid join SongRD on SongRD.gqid=Song.gqid order by times  asc");
                    var list = db.Database.SqlQuery<gs_gq_rd>(sql.ToString()).ToList();
                    Session["song_number"] = list.Count();
                    int pageNumber = page ?? 1;
                    int pageSize = 15;
                    IPagedList<gs_gq_rd> userPagedList = list.ToPagedList(pageNumber, pageSize);
                    return View(userPagedList);

                } if (bdid == 3)
                {
                    //流行
                    sql.Clear();
                    sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid join Songtype on Songtype.typeid=Song.typeid  join SongRD on SongRD.gqid=Song.gqid  where Songtype.typename='流行' order by times  desc ");
                    var list = db.Database.SqlQuery<gs_gq_rd>(sql.ToString()).ToList();
                    Session["song_number"] = list.Count();
                    int pageNumber = page ?? 1;
                    int pageSize = 15;
                    IPagedList<gs_gq_rd> userPagedList = list.ToPagedList(pageNumber, pageSize);
                    return View(userPagedList);
                }

            }
            return View();
        }


        /// <summary>
        /// 歌手
        /// </summary>
        /// <returns></returns>
        public ActionResult singer(int? page)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                var list = db.Singer.ToList();
                int pageNumber = page ?? 1;
                int pageSize = 15;
                IPagedList<Singer> userPagedList = list.ToPagedList(pageNumber, pageSize);
                return View(userPagedList);
            }
        }

        /// <summary>
        /// 最新音乐
        /// </summary>
        /// <returns></returns>
        public ActionResult zxmusic()
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                sql.Clear();
                sql.AppendFormat("select top 10* from Song join  Singer on Singer.gsid=Song.gsid join Zhuanj on Zhuanj.gsid=Song.gsid order by times  desc");
                var list = db.Database.SqlQuery<gs_gq_zj>(sql.ToString()).ToList();
                var lis = list.GroupBy(c => c.zjname).Select(c => c.First()).ToList();
                for (int i = 0; i < lis.Count; i++)
                {
                    lis[i].Zhuanjimg = "/Zhuanjimg/" + lis[i].Zhuanjimg;
                }

                sql.Clear();
                sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid join Zhuanj on Zhuanj.gsid=Song.gsid order by zjhot  desc");
                var list1 = db.Database.SqlQuery<gs_gq_zj>(sql.ToString()).ToList();
                var list2 = list1.GroupBy(c => c.zjname).Select(c => c.First()).ToList();
                for (int i = 0; i < list2.Count; i++)
                {
                    list2[i].Zhuanjimg = "/Zhuanjimg/" + list2[i].Zhuanjimg;
                }
                ViewData["hotzj"] = list2.Take(10).ToList();
                return View(lis);
            }
        }

        /// <summary>
        /// 音乐人
        /// </summary>
        /// <returns></returns>
        public ActionResult yinyuer()
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                var s = int.Parse(Session["id"].ToString());
                var list = db.UserSinger.Where(a => a.useriid == s).ToList();
                int gsid_1 = int.Parse(list.ElementAt(0).gsid.ToString());
                var id = db.Song.Where(c => c.gsid == gsid_1).ToList();
                if (id.Count < 1)
                {
                    Session["gq"] = 1;
                    sql.Clear();
                    //因为不传值model会报错，就让随便传了一个，前台不显示就行了
                    sql.AppendFormat("select * from UserSinger join Singer on Singer.gsid=UserSinger.gsid join Song on Song.gsid=UserSinger.gsid");
                    var list1 = db.Database.SqlQuery<yh_gs_gq>(sql.ToString()).ToList();
                    return View(list1);
                }
                else
                {
                    int gsid = int.Parse(list.ElementAt(0).gsid.ToString());
                    Session["gq"] = 0;
                    sql.Clear();
                    sql.AppendFormat("select * from UserSinger join Singer on Singer.gsid=UserSinger.gsid join Song on Song.gsid=UserSinger.gsid where UserSinger.gsid={0}", gsid);
                    var list2 = db.Database.SqlQuery<yh_gs_gq>(sql.ToString()).ToList();
                    return View(list2);
                }
            }
        }
        #region   音乐人替换
        /// <summary>
        /// 音乐上传
        /// </summary>
        /// <param name="gqimg"></param>
        /// <param name="Zhuanjimg"></param>
        /// <param name="mp3"></param>
        /// <param name="gqname"></param>
        /// <param name="typename"></param>
        /// <param name="js"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult yinyuer(HttpPostedFileBase gqimg, HttpPostedFileBase Zhuanjimg, HttpPostedFileBase mp3, string gqname, int typename, string zjname, string js, string gc)
        {
            if (Request.Files.Count > 0 && gqimg != null)
            {
                //保存图片到指定文件夹
                HttpPostedFileBase f = Request.Files["gqimg"];
                f.SaveAs(Server.MapPath("~/gqimg/") + f.FileName);
                HttpPostedFileBase f1 = Request.Files["Zhuanjimg"];
                f1.SaveAs(Server.MapPath("~/Zhuanjimg/") + f1.FileName);
                HttpPostedFileBase f2 = Request.Files["mp3"];
                f2.SaveAs(Server.MapPath("~/mp3/") + f2.FileName);

                //获取图片名称
                string gqimg_1 = gqimg.FileName;
                string Zhuanjimg_1 = Zhuanjimg.FileName;
                string mp3_1 = mp3.FileName;
                using (YIYUNEntities15 db = new YIYUNEntities15())
                {
                    var s = int.Parse(Session["id"].ToString());
                    var list = db.Users.Where(a => a.userid == s).ToList();
                    string userid = list.ElementAt(0).userid.ToString();
                    string username = list.ElementAt(0).username.ToString();
                    string usesrimg = list.ElementAt(0).usersimg.ToString();

                    //歌手账号只需要添加一次——判断歌手账号是否存在
                    int cz = db.Singer.Where(c => c.gsname == username).Count();
                    if (cz < 1)
                    {
                        //添加歌手账号
                        sql.Clear();
                        sql.AppendFormat("insert  into Singer values('{0}','{1}')", username, usesrimg);
                        int d = db.Database.ExecuteSqlCommand(sql.ToString());

                        var list1 = db.Singer.Where(a => a.gsname == username).ToList();
                        int gsid = list1.ElementAt(0).gsid;
                        sql.Clear();
                        sql.AppendFormat("insert into UserSinger values({0},{1})", s, gsid);
                        int b = db.Database.ExecuteSqlCommand(sql.ToString());
                    }

                    //添加1——添加专辑
                    var list4 = db.Singer.Where(a => a.gsname == username).ToList();
                    int gsid1 = list4.ElementAt(0).gsid;
                    sql.Clear();
                    sql.AppendFormat(" insert into Zhuanj values({0},'{1}',0,0,'{2}',0)", gsid1, zjname, Zhuanjimg_1);
                    int j = db.Database.ExecuteSqlCommand(sql.ToString());

                    //添加2——添加歌曲
                    sql.Clear();
                    sql.AppendFormat("SELECT *  FROM [YIYUN].[dbo].[Zhuanj]  where gsid={0} and zjname='{1}'", gsid1, zjname);
                    var list2 = db.Database.SqlQuery<Zhuanj>(sql.ToString()).ToList();
                    var zjid = list2.ElementAt(0).zjid;
                    sql.Clear();
                    sql.AppendFormat("insert into Song(zjid,gqimg,js,jb,gqname,typeid,texts,sc,times,gsid,userid,mp3) values({0},'{1}','{2}',0,'{3}',{4},'{5}','通过',GETDATE(),{6},{7},'{8}')", zjid, gqimg_1, js, gqname, typename, gc, gsid1, 0, mp3_1);
                    int e = db.Database.ExecuteSqlCommand(sql.ToString());


                    //添加3——添加歌曲热度
                    sql.Clear();
                    sql.AppendFormat("select * from Song where zjid={0} and gqname='{1}'", zjid, gqname);
                    var gq = db.Database.SqlQuery<Song>(sql.ToString()).ToList();
                    int gqid = gq.ElementAt(0).gqid;
                    sql.Clear();
                    sql.AppendFormat("SELECT * FROM [YIYUN].[dbo].[SongRD]  where gqid={0}", gqid);
                    var gqrd = db.Database.SqlQuery<SongRD>(sql.ToString()).ToList();
                    int count = gqrd.Count();
                    if (count != 1)
                    {
                        sql.Clear();
                        sql.AppendFormat("INSERT [dbo].[SongRD] ([gqid], [gqhot]) values ({0},{1})", gqid, 0);
                        int p = db.Database.ExecuteSqlCommand(sql.ToString());
                    }

                    //添加——歌手热度
                    sql.Clear();
                    sql.AppendFormat("SELECT *  FROM [YIYUN].[dbo].[SingerRD]  where gsid={0}", gsid1);
                    var gsrd = db.Database.SqlQuery<SingerRD>(sql.ToString()).ToList();
                    int count_1 = gsrd.Count();
                    if (count_1 < 1)
                    {
                        //添加4
                        sql.Clear();
                        sql.AppendFormat("INSERT INTO [YIYUN].[dbo].[SingerRD]([gsid],[gshot])VALUES({0}, {1})", gsid1, 0);
                        int m = db.Database.ExecuteSqlCommand(sql.ToString());
                    }

                    //添加songlist
                    sql.Clear();
                    sql.AppendFormat("INSERT INTO [YIYUN].[dbo].[Songlist]([gdname],[userid],[gdimg])VALUES('{0}',{1},'{2}')", zjname, gsid1, Zhuanjimg_1);
                    int sl = db.Database.ExecuteSqlCommand(sql.ToString());

                    //添加添加songlists
                    if (sl > 0)
                    {
                        var sls = db.Songlist.Where(a => a.userid == gsid1).ToList();
                        int gdid = sls.ElementAt(0).gdid;
                        sql.Clear();
                        sql.AppendFormat("INSERT INTO [YIYUN].[dbo].[Songlistss] ([gdid] ,[gqid])  VALUES ({0},{1})", gdid, gqid);
                        db.Database.ExecuteSqlCommand(sql.ToString());
                    }

                    //判断
                    if (j == 1 && e == 1)
                    {
                        //弹窗，并跳转
                        return Content("<script>alert('添加成功！');window.location.href='../Home/yinyuer';</script>");
                    }
                    else
                    {
                        //弹窗，并跳转
                        return Content("<script>alert('添加失败，部分数据添加错误！');window.location.href='../Home/yinyuer';</script>");
                    }
                }
            }
            return View();
        }

        #endregion

        /// <summary>
        /// 歌手详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult singer_yq(int czid)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                sql.Clear();
                sql.AppendFormat("select top 10* from Song join  Singer on Singer.gsid=Song.gsid join Zhuanj on Zhuanj.gsid=Song.gsid  where singer.gsid={0} order by  zjhot desc", czid);
                var list = db.Database.SqlQuery<gs_gq_zj>(sql.ToString()).ToList();
                var list1 = list.GroupBy(c => c.zjname).Select(c => c.First()).ToList();

                var namelist = db.Singer.FirstOrDefault(c => c.gsid == czid);
                ViewBag.name = namelist.gsname;

                sql.Clear();
                sql.AppendFormat("select * from Song join  Singer on Singer.gsid=Song.gsid where singer.gsid={0}", czid);
                var gqcount = db.Database.SqlQuery<gs_gq>(sql.ToString()).ToList();
                ViewBag.count = gqcount.Count;

                sql.Clear();
                sql.AppendFormat("select * from Song join SongRD on SongRD.gqid=Song.gqid where Song.gsid={0}", czid);
                var gqhot = db.Database.SqlQuery<song_songrd>(sql.ToString()).ToList();
                ViewBag.gqhot = gqhot.Sum(c => c.gqhot).ToString();

                return View(list1);
            }
        }

        #endregion

        #region   跳转到隐藏页
        /// <summary>
        /// 轮播详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult lunbotj()
        {
            return View();
        }
        /// <summary>
        /// 热门推荐详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult remtj(int remid)
        {
            //歌曲列表linq语句连表查询     
            var newgxtj = from a in db.Songlist
                          join b in db.Users on a.userid equals b.userid
                          select new remtjtiaozhuan
                          {
                              gdimg = a.gdimg,
                              userimg = b.usersimg,
                              username = b.username,
                              gdname = a.gdname,
                              gdid = a.gdid
                          } into gxtj
                          select gxtj;
            var newzjtj1 = newgxtj.Where(p => p.gdid == remid).ToList();
            ViewData["remtj"] = newzjtj1;
            //歌单
            var ged = from a in db.Songlist
                      join b in db.Songlistss on a.gdid equals b.gdid
                      join c in db.Song on b.gqid equals c.gqid
                      join d in db.Zhuanj on c.zjid equals d.zjid
                      join e in db.Singer on c.gsid equals e.gsid
                      select new gedansong
                      {

                          gqid = c.gqid,
                          gdid = a.gdid,
                          gq = c.gqname,
                          mp3 = c.mp3,
                          zjname = d.zjname,
                          gsname = e.gsname
                      } into gd
                      select gd;
            var ged1 = ged.Where(p => p.gdid == remid).ToList();
            ViewData["ged"] = ged1;
            return View();
        }
        /// <summary>
        /// 专辑推荐详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult zjtj(int zjid)
        {
            var item = from a in db.Zhuanj
                       join b in db.Singer on a.gsid equals b.gsid
                       join c in db.Song on a.zjid equals c.zjid
                       select new rmtjzjtj
                       {
                           zjhot = a.zjhot,
                           zjid = a.zjid,
                           zjname = a.zjname,
                           zjimg = a.Zhuanjimg,
                           gsimg = b.gsimg,
                           gsname = b.gsname,
                           gsid = b.gsid
                           //zjmoney = a.gqmoney              
                       } into items
                       select items;
            var zjtj = item.Where(p => p.zjid == zjid).Distinct().ToList();
            ViewData["zjtj"] = zjtj;
            //歌单
            var ged = from a in db.Songlist
                      join b in db.Songlistss on a.gdid equals b.gdid
                      join c in db.Song on b.gqid equals c.gqid
                      join d in db.Zhuanj on c.zjid equals d.zjid
                      join e in db.Singer on c.gsid equals e.gsid
                      select new gedansong
                      {
                          gqid = c.gqid,
                          zjid = d.zjid,
                          gq = c.gqname,
                          mp3 = c.mp3,
                          zjname = d.zjname,
                          gsname = e.gsname
                      } into gd
                      select gd;
            var ged1 = ged.Where(p => p.zjid == zjid).Distinct().ToList();
            ViewData["ged"] = ged1;
            return View();
        }
        public ActionResult sczj(int zjid)
        {

            int userid = Convert.ToInt32(Session["userid"]);
            ViewBag.userid = userid;
            if (userid == 0)
            {
                Response.Write(" <script>alert('请登录后再来点赞吧！')</script>");
            }
            else
            {
                var sczj = db.Zhuanj.FirstOrDefault(p => p.zjid == zjid);
                sczj.zjhot = sczj.zjhot + 1;
                db.SaveChanges();
            }
            zjtj(zjid);
            return View("zjtj");
        }
        /// <summary>
        /// 名气歌手详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult mqgs(int gsid)
        {
            var item = from a in db.Singer
                       join b in db.SingerLX on a.gsid equals b.gslxid
                       join c in db.SingerRD on a.gsid equals c.gsid
                       select new rmtjmqgs
                       {

                           gsid = a.gsid,
                           gsimg = a.gsimg,
                           gsname = a.gsname,
                           gstype = b.gstype,
                           gshot = c.gshot

                       } into items
                       select items;
            var mqgs = item.Where(p => p.gsid == gsid).Distinct().ToList();
            ViewData["mqgs"] = mqgs;
            //歌单
            var ged = from a in db.Songlist
                      join b in db.Songlistss on a.gdid equals b.gdid
                      join c in db.Song on b.gqid equals c.gqid
                      join d in db.Zhuanj on c.zjid equals d.zjid
                      join e in db.Singer on c.gsid equals e.gsid
                      select new gedansong
                      {
                          gqid = c.gqid,
                          gsid = d.gsid,
                          gq = c.gqname,
                          mp3 = c.mp3,
                          zjname = d.zjname,
                          gsname = e.gsname

                      } into gd
                      select gd;
            var ged1 = ged.Where(p => p.gsid == gsid).Distinct().ToList();
            ViewData["ged"] = ged1;

            return View();
        }
        public ActionResult dzgs(int gsid)
        {
            int userid = Convert.ToInt32(Session["userid"]);
            ViewBag.userid = userid;
            if (userid == 0)
            {
                Response.Write(" <script>alert('请登录后再来点赞吧！')</script>");
            }
            else
            {
                var sczj = db.SingerRD.FirstOrDefault(p => p.gsid == gsid);
                sczj.gshot = sczj.gshot + 1;
                db.SaveChanges();
            }
            mqgs(gsid);
            return View("mqgs");
        }
        /// <summary>
        /// 歌曲详情页
        /// </summary>
        /// <returns></returns>
        public ActionResult gequ(int gqid)
        {
            var item = from a in db.Song
                       join b in db.Singer on a.gsid equals b.gsid
                       join c in db.Songtype on a.typeid equals c.typeid
                       join d in db.Zhuanj on a.zjid equals d.zjid
                       join e in db.SongRD on a.gqid equals e.gqid
                       select new gequxq
                       {
                           zjid = d.zjid,
                           gsid = b.gsid,
                           zjimg = d.Zhuanjimg,
                           gqid = a.gqid,
                           gq = a.gqname,
                           img = a.gqimg,
                           js = a.js,
                           texts = a.texts,
                           times = a.times,
                           gqhot = e.gqhot,
                           jbtest = a.jbtest,
                           mp3 = a.mp3,
                           gsname = b.gsname,
                           typename = c.typename,
                           zjname = d.zjname,

                       } into s
                       select s;
            var gequ = item.Where(p => p.gqid == gqid).ToList();
            ViewData["gequ"] = gequ;
            Session["zonggequ"] = item.Count();
            var list = from a in db.Song
                       join b in db.SongRD on a.gqid equals b.gqid
                       select new gqredu
                       {
                           gqimg = a.gqimg,
                           gqhot = b.gqhot,
                           gqname = a.gqname,
                           gqid = a.gqid
                       } into ss
                       select ss;
            var gqzong = list.OrderByDescending(a => new { a.gqhot }).ToList();
            Session["gqzong"] = gqzong;

            int userid = Convert.ToInt32(Session["userid"]);
            ViewBag.userid = userid;
            //收藏点击后的查询
            var shouc = from a in db.Songlist
                        select new shouchang
                          {
                              gdid = a.gdid,
                              gdimg = a.gdimg,
                              gdname = a.gdname,
                              userid = a.userid
                          } into shoucc
                        select shoucc;

            var shouc1 = shouc.Where(p => p.userid == userid).ToList();
            ViewBag.gqid = gqid;
            ViewData["shouc"] = shouc1;
            //MV显示
            var mv = from a in db.MVtype
                     join b in db.Song on a.gqid equals b.gqid
                     select new MV
                     {
                         gqid = b.gqid,
                         MVlj = a.MV,
                         MVid = a.MVid
                     } into mvv
                     select mvv;
            var mv1 = mv.Where(p => p.gqid == gqid).ToList();
            if (mv1.Count() > 0)
            {
                ViewBag.MVid = mv1[0].MVid;
            }

            return View();
        }
        //上一首
        public ActionResult prev(int gqid)
        {
            int zonggequ = Convert.ToInt32(Session["zonggequ"]);
            if (gqid == 1)
            {
                gqid = zonggequ;
            }
            else
            {
                gqid = gqid - 1;
            }

            var item = from a in db.Song
                       join b in db.Singer on a.gsid equals b.gsid
                       join c in db.Songtype on a.typeid equals c.typeid
                       join d in db.Zhuanj on a.zjid equals d.zjid
                       select new gequxq
                       {
                           gqid = a.gqid,
                           gq = a.gqname,
                           img = a.gqimg,
                           js = a.js,
                           texts = a.texts,
                           times = a.times,

                           jbtest = a.jbtest,
                           mp3 = a.mp3,
                           gsname = b.gsname,
                           typename = c.typename,
                           zjname = d.zjname
                       } into s
                       select s;
            var nextgequ = item.Where(p => p.gqid == gqid).ToList();
            ViewData["gequ"] = nextgequ;

            gequ(gqid);
            return View("gequ");
        }
        //下一首
        public ActionResult next(int gqid)
        {
            int zonggequ = Convert.ToInt32(Session["zonggequ"]);
            if (gqid == zonggequ)
            {
                gqid = 1;
            }
            else
            {
                gqid = gqid + 1;
            }
            var item = from a in db.Song
                       join b in db.Singer on a.gsid equals b.gsid
                       join c in db.Songtype on a.typeid equals c.typeid
                       join d in db.Zhuanj on a.zjid equals d.zjid
                       select new gequxq
                       {
                           gqid = a.gqid,
                           gq = a.gqname,
                           img = a.gqimg,
                           js = a.js,
                           texts = a.texts,
                           times = a.times,

                           jbtest = a.jbtest,
                           mp3 = a.mp3,
                           gsname = b.gsname,
                           typename = c.typename,
                           zjname = d.zjname
                       } into s
                       select s;
            var nextgequ = item.Where(p => p.gqid == gqid).ToList();
            ViewData["gequ"] = nextgequ;

            gequ(gqid);
            return View("gequ");
        }
        //收藏
        public ActionResult shouc(int gqid, int gdid)
        {
            Songlistss newgdtj = new Songlistss();
            newgdtj.gqid = gqid;
            newgdtj.gdid = gdid;
            //收藏点击后的查询
            var shouc = from a in db.Songlist
                        join b in db.Songlistss on a.gdid equals b.gdid
                        join c in db.SongRD on a.gdid equals c.gqid
                        select new shouchang
                          {
                              gdid = a.gdid,
                              gdimg = a.gdimg,
                              gdname = a.gdname,
                              gqid = b.gqid,
                              userid = a.userid,
                              gqhot = c.gqhot
                          } into shoucc
                        select shoucc;
            var shouc1 = shouc.Where(p => p.gdid == gdid).ToList();


            if (shouc1.Count == 0)
            {
                db.Songlistss.Add(newgdtj);
                db.SaveChanges();
                Response.Write(" <script>alert('您已收藏成功！')</script>");
                var rd = db.SongRD.FirstOrDefault(p => p.gqid == gqid);
                rd.gqhot += 1;
                db.SaveChanges();
            }
            for (int i = 0; i < shouc1.Count(); i++)
            {
                int id = shouc1[i].gqid;
                if (id == gqid)
                {
                    Response.Write(" <script>alert('您已收藏过这首歌，换首歌收藏吧！')</script>");
                    break;
                }
                else if (i == shouc1.Count - 1)
                {
                    db.Songlistss.Add(newgdtj);
                    db.SaveChanges();
                    Response.Write(" <script>alert('您已收藏成功！')</script>");
                    var rd = db.SongRD.FirstOrDefault(p => p.gqid == gqid);
                    rd.gqhot += 1;
                    db.SaveChanges();
                    break;
                }
            }


            gequ(gqid);
            return View("gequ");
        }
        //MV
        public ActionResult MV(int gqid)
        {
            var item = from a in db.MVtype
                       join b in db.Song on a.gqid equals b.gqid
                       join c in db.Singer on b.gsid equals c.gsid
                       select new MV
                       {
                           gqid = b.gqid,
                           MVlj = a.MV,
                           MVid = a.MVid,
                           gqname = b.gqname,
                           gsname = c.gsname
                       } into mv
                       select mv;
            var item1 = item.Where(p => p.gqid == gqid).ToList();
            ViewData["MV"] = item1;

            var tjmv = from a in db.Song
                       join c in db.MVtype on a.gqid equals c.gqid
                       select new MV
                       {
                           MVid = c.MVid,
                           gqid = a.gqid,
                           gqimg = a.gqimg,
                       } into mvv
                       select mvv;
            var tjmv1 = tjmv.OrderByDescending(p => new { p.MVid }).Take(6).ToList();

            ViewData["TJMV"] = tjmv1;
            return View();
        }
        #endregion

        #endregion

        #region 用户详情
        /// <summary>
        /// 用户个人界面
        /// </summary>
        /// <returns></returns>
        public ActionResult yh_xq()
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                var s = int.Parse(Session["userid"].ToString());
                var list = db.Users.Where(c => c.userid == s).ToList();
                return View(list);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="gdname"></param>
        /// <returns></returns>
        public ActionResult yh_xqsctx(HttpPostedFileBase file, string x, string age, string nic)
        {
            int userid = Convert.ToInt32(Session["userid"]);

            int newage = Convert.ToInt32(age);
            string iamg = Server.MapPath("/userimg/" + file.FileName);
            file.SaveAs(iamg);
            //newuser.age = age;
            var item = db.Users.FirstOrDefault(p => p.userid == userid);
            item.username = nic;
            item.usersimg = file.FileName;
            item.age = newage;
            item.sex = x;
            db.SaveChanges();
            yh_xq();
            return View("yh_xq");
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public ActionResult yh_tc()
        {
            Session["id"] = 0;
            gxtj();
            return View("gxtj");
        }


        /// <summary>
        /// 用户密码修改
        /// </summary>
        /// <returns></returns>
        public ActionResult yh_mmxg()
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                var s = int.Parse(Session["userid"].ToString());
                var list = db.Users.FirstOrDefault(c => c.userid == s);
                string name = list.username;
                Session["name"] = name;
                return View();
            }
        }
        [HttpPost]
        public ActionResult yh_mmxg(string userpwd, string userpwd_1)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                var s = int.Parse(Session["userid"].ToString());
                sql.Clear();
                if (userpwd == userpwd_1)
                {
                    sql.AppendFormat("update Users set userpwd={0} where userid={1}", userpwd, s);
                    db.Database.ExecuteSqlCommand(sql.ToString());
                    return Content("<script>alert('密码修改成功！请重新登录');window.location.href='../Home/denglu';</script>");
                }
                else
                {
                    return Content("<script>alert('两次密码输入不相同，需要重新输入');window.location.href='../Home/yh_mmxg';</script>");
                }
            }
        }

        /// <summary>
        /// 用户稿件管理
        /// </summary>
        /// <returns></returns>
        public ActionResult yh_gjgl()
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                int s = int.Parse(Session["userid"].ToString());
                var gsid = db.UserSinger.Where(b => b.useriid == s).ToList();
                int gsid_1 = int.Parse(gsid.ElementAt(0).gsid.ToString());
                var list = db.Song.Where(a => a.gsid == gsid_1).ToList();
                if (list.Count < 1)
                {
                    Response.Write(" <script>alert('您还未发布歌曲，先发布歌曲再来看看吧！')</script>");
                    yinyuer();
                    return View("yinyuer");
                }
                else
                {
                    sql.Clear();
                    sql.AppendFormat("select * from UserSinger join Singer on Singer.gsid=UserSinger.gsid join Song on Song.gsid=UserSinger.gsid where useriid={0}", s);
                    var list1 = db.Database.SqlQuery<yh_gs_gq>(sql.ToString()).ToList();
                    return View(list1);
                }
            }
        }

        /// <summary>
        /// 用户稿件管理——删除
        /// </summary>
        /// <returns></returns>
        public ActionResult yh_gjgl_del(int gqid, int zjid)
        {
            using (YIYUNEntities15 db = new YIYUNEntities15())
            {
                sql.Clear();
                sql.AppendFormat("DELETE FROM [YIYUN].[dbo].[Song]WHERE gqid={0}", gqid);
                int del_1 = db.Database.ExecuteSqlCommand(sql.ToString());

                sql.Clear();
                sql.AppendFormat("DELETE FROM [YIYUN].[dbo].[SongRD]WHERE gqid={0}", gqid);
                int del_2 = db.Database.ExecuteSqlCommand(sql.ToString());

                sql.Clear();
                sql.AppendFormat("DELETE FROM [YIYUN].[dbo].[Zhuanj]WHERE zjid={0}", zjid);
                int del_3 = db.Database.ExecuteSqlCommand(sql.ToString());


                int userid = int.Parse(Session["id"].ToString());
                var list = db.UserSinger.Where(a => a.useriid == userid).ToList();
                int gsid = int.Parse(list.ElementAt(0).gsid.ToString());
                var list_1 = db.Songlist.Where(c => c.userid == gsid).ToList();
                int gdid = int.Parse(list_1.ElementAt(0).gdid.ToString());
                sql.Clear();
                sql.AppendFormat("DELETE FROM [YIYUN].[dbo].[Songlist] WHERE userid={0}", gsid);
                int del_4 = db.Database.ExecuteSqlCommand(sql.ToString());

                sql.Clear();
                sql.AppendFormat("DELETE FROM [YIYUN].[dbo].[Songlistss]  WHERE gdid={0}", gdid);
                int del_5 = db.Database.ExecuteSqlCommand(sql.ToString());

                if (del_1 > 0 && del_2 > 0 && del_3 > 0 && del_4 > 0 && del_5 > 0)
                {
                    return Content("<script>alert('稿件删除成功！');window.location.href='../Home/yh_gjgl';</script>");
                }
                else
                {
                    return Content("<script>alert('稿件删除失败！');window.location.href='../Home/yh_gjgl';</script>");
                }

            }
        }






        #endregion


        #endregion

        #region  管理员界面

        /// <summary>
        /// 管理员界面
        /// </summary>
        /// <returns></returns>
        public ActionResult guanli()
        {
            return View();
        }

        /// <summary>
        /// 显示用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult updateuser()
        {
            var list = db.Users.ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult updateuser(string username)
        {
            var list = db.Users.Where(p => p.username.Contains(username)).ToList();
            return View(list);
        }
        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult del(int id)
        {
            var list = db.Users.FirstOrDefault(p => p.userid == id);
            if (list.vip == 1)
            {
                Response.Write("<script>alert('不能删除 VIP用户哦~！')</script>");
            }
            if (list.vip == 0)
            {
                db.Users.Remove(list);
                db.SaveChanges();
                Response.Write("<script>alert('删除成功！')</script>");
            }
            guanli();
            return View("guanli");
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult updates(int id)
        {
            var list = db.Users.Where(p => p.userid == id).ToList();
            return View(list);
        }
        public ActionResult update(Users user)
        {
            Users users = db.Users.FirstOrDefault(p => p.userid == user.userid);
            users.username = user.username;
            users.userpwd = user.userpwd;
            users.sex = user.sex;
            users.age = user.age;
            users.brith = user.brith;
            db.SaveChanges();
            Response.Write("<script>alert('修改成功！')</script>");
            guanli();
            return View("guanli");
        }

        /// <summary>
        /// 添加专辑信息
        /// </summary>
        /// <returns></returns>
        public ActionResult insertZhuanji()
        {
            var list = db.Singer.ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult insertZhuanji(Zhuanj zhuanji, int gsid)
        {
            Zhuanj zj = new Zhuanj();
            zj.gsid = gsid;
            zj.zjname = zhuanji.zjname;
            zj.picezj = 0;
            zj.zjhot = 0;
            //接收图片
            HttpPostedFileBase file = Request.Files["file"];
            string filename = System.IO.Path.GetExtension(file.FileName);
            //判断扩展名
            if (filename != ".jpg")
            {
                //返回前一页
                Response.Write("<script>alert('请添加后缀名为 .jpg 的头像吧~！')</script>");
            }
            else
            {
                string imgname = "~/img/" + file.FileName;
                file.SaveAs(Server.MapPath(imgname));
                zj.Zhuanjimg = file.FileName;
                //上传路径
                zj.gqmoney = 0;
                db.Zhuanj.Add(zj);
                db.SaveChanges();
                Response.Write("<script>alert('添加成功！')</script>");
            }
            guanli();
            return View("guanli");
        }
        /// <summary>
        /// 添加歌手
        /// </summary>
        /// <returns></returns>
        public ActionResult insertsinger()
        {
            var list = db.Songtype.ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult insertsinger(Singer singer)
        {
            Singer sing = new Singer();
            sing.gsname = singer.gsname;
            //接收图片
            HttpPostedFileBase file = Request.Files["file"];
            string filename = System.IO.Path.GetExtension(file.FileName);
            //判断扩展名
            if (filename != ".jpg")
            {
                //返回前一页
                Response.Write("<script>alert('请添加后缀名为 .jpg 的头像吧~！')</script>");
            }
            else
            {
                string imgname = "~/img/" + file.FileName;
                file.SaveAs(Server.MapPath(imgname));
                sing.gsimg = file.FileName;
                //上传路径
                db.Singer.Add(sing);
                db.SaveChanges();
                Response.Write("<script>alert('添加成功！')</script>");
            }
            guanli();
            return View("guanli");
        }
        /// <summary>
        /// 音乐上传
        /// </summary>
        /// <returns></returns>
        public ActionResult insertSong()
        {
            var list = db.Zhuanj.ToList();
            ViewData["item"] = db.Songtype.ToList();
            ViewData["type"] = db.Song.ToList();
            ViewData["sing"] = db.Singer.ToList();

            return View(list);
        }
        [HttpPost]
        public ActionResult insertSong(Song song, int zjid, int typeid, int gsid, int userid)
        {
            string users = this.Session["userid"].ToString();
            Song son = new Song();
            son.zjid = zjid;
            son.typeid = typeid;
            son.js = song.js;
            son.gqname = song.gqname;
            son.texts = song.texts;
            son.times = song.times;
            son.sc = "通过";
            son.gsid = gsid;
            son.userid = Convert.ToInt32(userid);
            //图片文件
            HttpPostedFileBase file = Request.Files["file"];
            //图片文件名
            string filename = System.IO.Path.GetExtension(file.FileName);

            //音频文件
            HttpPostedFileBase file2 = Request.Files["file2"];
            //音频文件名
            string mp3filename = System.IO.Path.GetExtension(file2.FileName);

            //判断扩展名
            if (filename != ".jpg")
            {
                Response.Write("<script>alert('请添加后缀名为 .jpg 的头像吧~！')</script>");
            }

            if (mp3filename != ".mp3")
            {
                Response.Write("<script>alert('请添加后缀名为 .mp3 的音乐吧~！')</script>");
            }
            else
            {
                //图片上传
                string imgname = "~/gqimg/" + file.FileName;
                file.SaveAs(Server.MapPath(imgname));
                son.gqimg = file.FileName;
                //音频上传
                string mp3name = "~/mp3/" + file2.FileName;
                file2.SaveAs(Server.MapPath(mp3name));
                son.mp3 = file2.FileName;
                db.Song.Add(son);
                db.SaveChanges();
                Response.Write("<script>alert('上传成功！')</script>");
            }
            guanli();
            return View("guanli");
        }
        /// <summary>
        /// 添加MV
        /// </summary>
        /// <returns></returns>
        public ActionResult insertMV()
        {
            var list = db.Song.ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult insertMV(int gqid, MVtype type)
        {

            var mv = db.MVtype.ToList();
            for (int i = 0; i < mv.Count; i++)
            {
                if (mv[i].gqid==gqid)
                {
                    Response.Write("<script>alert('这首歌已经有mv了，换一首试试吧！')</script>");
                    break;
                }
                else if (i == mv.Count - 1 && mv[i].gqid != gqid)
                {
                    MVtype a = new MVtype();
                    a.gqid = gqid;
                    //MV视频文件
                    HttpPostedFileBase file = Request.Files["file"];
                    //MV视频文件名
                    string mvfile = System.IO.Path.GetExtension(file.FileName);
                    //判断扩展名
                    if (mvfile != ".mp4")
                    {
                        //返回前一页
                        Response.Write("<script>alert('请添加后缀名为 .mp4 的MV视频吧~！')</script>");
                    }
                    //MV上传
                    else
                    {
                        string mp4name = "~/mp4/" + file.FileName;
                        file.SaveAs(Server.MapPath(mp4name));
                        a.MV = file.FileName;
                        db.MVtype.Add(a);
                        db.SaveChanges();
                        Response.Write("<script>alert('MV视频添加成功！')</script>");
                    }

                    guanli();
                    return View("guanli");
                }
            }
            guanli();
            return View("guanli");
            
        }
        public ActionResult insertSongRD()
        {
            var list = db.Song.ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult insertSongRD(int gqid, int gqhot)
        {
            SongRD rd = new SongRD();
            rd.gqid = gqid;
            rd.gqhot = gqhot;
            db.SongRD.Add(rd);
            db.SaveChanges();
            guanli();
            return View("guanli");
        }
        /// <summary>
        /// 下架歌曲
        /// </summary>
        /// <returns></returns>
        public ActionResult deleteSong(int? page)
        {
            var list = db.Song.ToList();
            int pageNumber = page ?? 1;
            int pageSize = 4;
            IPagedList<Song> SongPagedList = list.ToPagedList(pageNumber, pageSize);
            return View(SongPagedList);
        }

        public ActionResult deleteS(int gqid)
        {
            var list = db.Song.FirstOrDefault(p => p.gqid == gqid);
           
            db.Song.Remove(list);
            var deletmv = db.MVtype.FirstOrDefault(p => p.gqid == gqid);
            db.MVtype.Remove(deletmv);
            db.SaveChanges();
            Response.Write("<script>alert('由于版权原因，当前歌曲已被强制要求下架！！！')</script>");
            guanli();
            return View("guanli");
        }
        #endregion

    }
}
