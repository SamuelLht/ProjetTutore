﻿using PlateauJeu.Class_Cartes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlateauJeu
{
    public partial class Form1 : Form
    {
        private byte m_manche;
        private byte m_nbPepite;
        private Joueur m_joueurActif;
        private bool m_dragDropDone;
        private bool m_mouseLeft;
        private Plateau m_Plateau;
        private Joueur m_Joueur1;
        private Joueur m_Joueur2;
        private PictureBox m_picDest;
        private PictureBox m_picSource;

        public Form1()
        {
            InitializeComponent();
            /// <summary>
            /// Gestionnaires d'évènements Drag and Drop
            /// Dimensionnement automatique des images
            /// </summary>
            foreach (PictureBox pic in tableLayoutPanel1.Controls)
            {
                pic.DragEnter += new DragEventHandler(pictureBox_DragEnter);
                pic.DragDrop += new DragEventHandler(pictureBox_DragDrop);
                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Dock = DockStyle.Fill;
            }
            /// <summary>
            /// Gestionnaire d'évènement du clic
            /// </summary>
            foreach (PictureBox pic in pnl_main.Controls)
            {
                pic.MouseDown += new MouseEventHandler(pictureBox_MouseDown);
                pic.MouseMove += new MouseEventHandler(pictureBox_MouseMove);
            }
            m_dragDropDone = false;
            m_manche = 0;
            m_nbPepite = 8;
            //m_Plateau = new Plateau();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /// <summary>
            /// Autorise le Drag and Drop
            /// </summary>
            foreach (PictureBox pic in tableLayoutPanel1.Controls)
            {
                pic.AllowDrop = true;
            }
            txt_J1.Text = "Tomori";
            txt_J2.Text = "Isla";
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            /// <summary>
            /// Evenement de clic déclenché si le DragAndDrop n'a pas encore été effectué
            /// </summary>
            if (e.Button == MouseButtons.Left && !m_dragDropDone)
            { 
                m_mouseLeft = true;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_mouseLeft)
            {
                /*
                 * Récupère le type de carte grâce au pointeur contenu dans le Tag de la PictureBox
                 */
                PictureBox v_pic1 = (PictureBox)sender;
                Type v_type = v_pic1.Tag.GetType();
                if (v_type.IsSubclassOf(typeof(CartePlacable)))
                {
                    /*
                     * Lance et attend la fin du DragAndDrop
                     */
                    if (v_pic1.DoDragDrop(v_pic1.Image, DragDropEffects.Copy) == DragDropEffects.Copy && m_dragDropDone)
                    {
                        /*
                         * Pointeur de la PictureBox source du DragAndDrop 
                         */
                        m_picSource = v_pic1;
                        /*
                         * Supprime l'image dans la PictureBox source et affecte le tag de la PictureBox destination avec la carte
                         */
                        v_pic1.Image = null;
                        m_picDest.Tag = v_pic1.Tag;
                        m_mouseLeft = false;
                    }
                }
            }
        }

        private void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox v_pic2 = (PictureBox)sender;
            if ((e.Data.GetDataPresent(DataFormats.Bitmap)))
            {
                /*
                 * Vérifie que la PictureBox de destination ne possède pas déjà une image
                 */
                if (v_pic2.Parent == tableLayoutPanel1 && v_pic2.Image == null)
                {
                    /*
                     * Affecte l'image de la PictureBox de destination, pointeur vers la PictureBox de destination et flag
                     */
                    Bitmap v_bitmap = (Bitmap)(e.Data.GetData(DataFormats.Bitmap));
                    v_pic2.Image = v_bitmap;
                    m_picDest = v_pic2;
                    m_dragDropDone = true;
                }
            }
        }

        private void btn_end_Click(object sender, EventArgs e)
        {
            if(m_manche == 0)
            {
                /*
                 * Vérifie l'affectation des noms
                 */
                if ((txt_J1.Text != "") && (txt_J2.Text != ""))
                {
                    /*
                     * Créé le Plateau, les joueurs, lance la manche 1, affiche les controles et place les cases départ
                     */ 
                    m_Plateau = new Plateau();
                    initJoueurs();
                    m_manche++;
                    afficherElements();
                    placerDeparts();
                }
            }
            /*
             * Vérifie si un DragAndDrop a été réalisé
             */
            else if(m_dragDropDone)
            {
                /*
                 * Récupère la carte et la retire de la main du joueur
                 * Réinitialise le pointeur de carte de la PictureBox source
                 * Le joueur pioche 1 carte
                 * flag baissé
                 */
                CartePlacable v_carte = (CartePlacable)m_picSource.Tag;
                m_joueurActif.RetirerCarteDeLaMain(m_Plateau, v_carte);
                m_picSource.Tag = null;
                m_joueurActif.Piocher(m_Plateau, 1);
                m_dragDropDone = false;
                /*
                 * Changement de joueur
                 */
                if (m_joueurActif == m_Joueur1)
                {
                    m_joueurActif = m_Joueur2;
                }
                else
                { 
                    m_joueurActif = m_Joueur1;
                }
            }
            /*
             * Mise à jour de l'affichage des compteurs et de la main du joueur
             */
            majCompteurs();
            majCartes();
        }
        /// <summary>
        /// Initialise les joueurs aléatoirement
        /// </summary>
        private void initJoueurs()
        {
            Random rand = new Random();
            byte v_aleatoire = (byte)rand.Next(1, 2);
            switch (v_aleatoire)
            {
                case 1:
                    m_Joueur1 = new Joueur(txt_J1.Text, Couleur.Vert, m_Plateau);
                    m_Joueur2 = new Joueur(txt_J2.Text, Couleur.Bleu, m_Plateau);
                    m_joueurActif = m_Joueur1;
                    break;
                case 2:
                    m_Joueur1 = new Joueur(txt_J1.Text, Couleur.Bleu, m_Plateau);
                    m_Joueur2 = new Joueur(txt_J2.Text, Couleur.Vert, m_Plateau);
                    m_joueurActif = m_Joueur2;
                    break;
            }
        }
        /// <summary>
        /// Met à jour l'affichage des compteurs
        /// </summary>
        private void majCompteurs()
        {
            lbl_manche.Text = "Manche " + m_manche + "/3";
            lbl_pepite.Text = "Pépites restantes : " + m_nbPepite;
            if (m_joueurActif == m_Joueur1)
            {
                lbl_tourDe.Text = "Tour Joueur 1";
            }
            else
            {
                lbl_tourDe.Text = "Tour Joueur 2";
            }
        }
        /// <summary>
        /// Affiche les contrôles
        /// </summary>
        private void afficherElements()
        {
            pnl_joueur.Visible = true;
            tableLayoutPanel1.Visible = true;
            lbl_manche.Visible = true;
            lbl_pepite.Visible = true;
            lbl_tourDe.Visible = true;
            txt_J1.Enabled = false;
            txt_J2.Enabled = false;
            btn_undo.Visible = true;
        }
        /// <summary>
        /// Met à jour la main du joueur
        /// </summary>
        private void majCartes()
        {
            for(int i=0; i<m_joueurActif.MainJoueur.Count; i++)
            {
                PictureBox pic = (PictureBox)pnl_main.Controls[i];
                pic.Image = m_joueurActif.getCarteAtPosition(i).ImgRecto;
                pic.Tag = m_joueurActif.getCarteAtPosition(i);
            }
        }
        /// <summary>
        /// Place les cartes départ
        /// </summary>
        private void placerDeparts()
        {
            PictureBox pic = (PictureBox)tableLayoutPanel1.GetControlFromPosition(2, 4);
            m_Plateau.Departs.ElementAt(0).Id = m_Plateau.Id++;
            pic.Image = m_Plateau.Departs.ElementAt(0).ImgRecto;
            pic.Tag = m_Plateau.Departs.ElementAt(0);
            pic = (PictureBox)tableLayoutPanel1.GetControlFromPosition(2, 6);
            m_Plateau.Departs.ElementAt(1).Id = m_Plateau.Id++;
            pic.Image = m_Plateau.Departs.ElementAt(1).ImgRecto;
            pic.Tag = m_Plateau.Departs.ElementAt(1);
        }

        private void btn_undo_Click(object sender, EventArgs e)
        {
            /*
             * Réinitialise la PictureBox de destination, baisse le flag et met à jour la main du joueur
             */
            if(m_dragDropDone)
            {
                m_picDest.Image = null;
                m_picDest.Tag = null;
                m_dragDropDone = false;
                majCartes();
            }
        }
    }
}
